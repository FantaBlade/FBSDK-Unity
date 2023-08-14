using System;
using FantaBlade.Platform.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.Platform.UI
{
    internal class MobileController : MonoBehaviour, IController
    {
        private static readonly CountryInfo[] CountryInfos = CountryInfo.CountryInfos;

        public string CountryCode
        {
            get { return CountryInfos[_callingCodes.value].CountryCodeIso2; }
        }

        public string MobileNumber
        {
            get { return _mobileNumber.text; }
            set { _mobileNumber.text = value; }
        }


        [SerializeField] private Dropdown _callingCodes;
        [SerializeField] private InputField _mobileNumber;

        private Text _dropDownCaptionText;

        public void Init()
        {
            _callingCodes.options.Clear();
            foreach (var countryInfo in CountryInfos)
            {
                _callingCodes.options.Add(
                    new Dropdown.OptionData(countryInfo.Name + "[Space]+" + countryInfo.Telephone));
            }

            if (!_dropDownCaptionText)
            {
                _dropDownCaptionText = _callingCodes.captionText;
                _callingCodes.captionText = null;
            }

            int index = SdkManager.Location != null ? CountryCodeToCountryInfoIndex(SdkManager.Location) : -1;
            if (index == -1)
            {
                index = CountryInfo.DefaultCountyIndex;
            }

            _callingCodes.value = index;
            _dropDownCaptionText.text = "+" + CountryInfos[index].Telephone; //_callingCodes.options[index].text;
            _callingCodes.onValueChanged.AddListener((idx) =>
            {
                SdkManager.Location = CountryInfos[idx].CountryCodeIso2;
                _dropDownCaptionText.text = "+" + CountryInfos[idx].Telephone; //_callingCodes.options[index].text;
            });

            SdkManager.LocationSuccess += OnLocationSuccess;
            
            if (!string.IsNullOrEmpty(SdkManager.CachePhoneNumber))
            {
                MobileNumber = SdkManager.CachePhoneNumber;
            }
        }

        public void SaveCache()
        {
            SdkManager.CachePhoneNumber = MobileNumber;
        }

        private void OnDestroy()
        {
            SdkManager.LocationSuccess -= OnLocationSuccess;
        }
        
        private void OnLocationSuccess(string countryCode)
        {
            var i = CountryCodeToCountryInfoIndex(countryCode);
            _callingCodes.value = i;
//            _callingCodes.captionText.text = "+" + CountryInfos[i].Telephone;//_callingCodes.options[i].text;
        }
        
        private int CountryCodeToCountryInfoIndex(string countryCode)
        {
            for (var i = 0; i < CountryInfos.Length; i++)
            {
                if (countryCode == CountryInfos[i].CountryCodeIso2)
                {
                    return i;
                }
            }

            return CountryInfo.DefaultCountyIndex;
        }
    }
}