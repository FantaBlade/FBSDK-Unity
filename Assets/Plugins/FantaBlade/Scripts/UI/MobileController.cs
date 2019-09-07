using System;
using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
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
        }


        [SerializeField] private Dropdown _callingCodes;
        [SerializeField] private InputField _mobileNumber;

        public void Init()
        {
            foreach (var countryInfo in CountryInfos)
            {
                _callingCodes.options.Add(
                    new Dropdown.OptionData(countryInfo.NameInChinese + " +" + countryInfo.Telephone));
            }


            int index = SdkManager.Location != null ? CountryCodeToCountryInfoIndex(SdkManager.Location) : -1;
            if (index == -1)
            {
                index = CountryInfo.DefaultCountyIndex;
            }
            _callingCodes.value = index;
            _callingCodes.captionText.text = _callingCodes.options[index].text;

            SdkManager.LocationSuccess += OnLocationSuccess;
        }

        private void OnDestroy()
        {
            SdkManager.LocationSuccess -= OnLocationSuccess;
        }

        private void OnLocationSuccess(string countryCode)
        {
            var i = CountryCodeToCountryInfoIndex(countryCode);
            _callingCodes.value = i;
            _callingCodes.captionText.text = _callingCodes.options[i].text;
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