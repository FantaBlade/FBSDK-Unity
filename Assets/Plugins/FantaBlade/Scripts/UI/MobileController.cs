using FantaBlade.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace FantaBlade.UI
{
    internal class MobileController : MonoBehaviour, IController
    {
        private const int DefaultCountyIndex = 8; // China

        private static string _countryInfoByIp;

        public string CountryCode
        {
            get { return _countryInfos[_callingCodes.value].CountryCodeIso2; }
        }

        public string MobileNumber
        {
            get { return _mobileNumber.text; }
        }


        [SerializeField] private Dropdown _callingCodes;
        [SerializeField] private InputField _mobileNumber;

        public void Init()
        {
            foreach (var countryInfo in _countryInfos)
            {
                _callingCodes.options.Add(
                    new Dropdown.OptionData(countryInfo.NameInChinese + " +" + countryInfo.Telephone));
            }

            if (_countryInfoByIp != null)
            {
                _callingCodes.captionText.text = _countryInfoByIp;
            }
            else
            {
                // 设为默认值
                _callingCodes.value = DefaultCountyIndex;
                _callingCodes.captionText.text = _callingCodes.options[DefaultCountyIndex].text;
                PlatformApi.Util.GetIpInfo.Get((err, info, response) =>
                {
                    if (err == null)
                    {
                        var countryCode = response.countryCode;
                        for (var i = 0; i < _countryInfos.Length; i++)
                        {
                            if (countryCode == _countryInfos[i].CountryCodeIso2)
                            {
                                _countryInfoByIp = _callingCodes.options[i].text;
                                _callingCodes.value = i;
                                _callingCodes.captionText.text = _countryInfoByIp;
                            }
                        }
                    }
                });
            }
        }

        public class CountryInfo
        {
            public readonly string Name;
            public readonly string NameInChinese;
            public readonly string CountryCodeIso2;
            public readonly string CountryCodeIso3;
            public readonly string Telephone;


            public CountryInfo(string name, string nameInChinese, string countryCodeIso2, string countryCodeIso3,
                string telephone)
            {
                Name = name;
                NameInChinese = nameInChinese;
                CountryCodeIso2 = countryCodeIso2;
                CountryCodeIso3 = countryCodeIso3;
                Telephone = telephone;
            }
        }

        private readonly CountryInfo[] _countryInfos =
        {
            new CountryInfo("Afghanistan", "阿富汗", "AF", "AFG", "93"),
            new CountryInfo("Armenia", "亚美尼亚", "AM", "ARM", "374"),
            new CountryInfo("Azerbaijan", "阿塞拜疆", "AZ", "AZE", "994"),
            new CountryInfo("Bahrain", "巴林", "BH", "BHR", "973"),
            new CountryInfo("Bangladesh", "孟加拉国", "BD", "BGD", "880"),
            new CountryInfo("Bhutan", "不丹", "BT", "BTN", "975"),
            new CountryInfo("Brunei Darussalam", "文莱", "BN", "BRN", "673"),
            new CountryInfo("Cambodia", "柬埔寨", "KH", "KHM", "855"),
            new CountryInfo("China", "中国", "CN", "CHN", "86"),
            new CountryInfo("Christmas Island", "澳大利亚圣诞岛", "CX", "CXR", "61"),
            new CountryInfo("Cocos (Keeling) Islands", "科科斯（基林）群岛", "CC", "CCK", "61"),
            new CountryInfo("Cyprus", "塞浦路斯", "CY", "CYP", "357"),
            new CountryInfo("Georgia", "格鲁吉亚", "GE", "GEO", "995"),
            new CountryInfo("Hong Kong", "中国香港特别行政区", "HK", "HKG", "852"),
            new CountryInfo("India", "印度", "IN", "IND", "91"),
            new CountryInfo("Indonesia", "印度尼西亚", "ID", "IDN", "62"),
            new CountryInfo("Iran", "伊朗", "IR", "IRN", "98"),
            new CountryInfo("Iraq", "伊拉克", "IQ", "IRQ", "964"),
            new CountryInfo("Israel", "以色列", "IL", "ISR", "972"),
            new CountryInfo("Japan", "日本", "JP", "JPN", "81"),
            new CountryInfo("Jordan", "约旦", "JO", "JOR", "962"),
            new CountryInfo("Kazakhstan", "哈萨克斯坦", "KZ", "KAZ", "7"),
            new CountryInfo("North Korea", "朝鲜", "KP", "PRK", "850"),
            new CountryInfo("South Korea", "韩国", "KR", "KOR", "82"),
            new CountryInfo("Kuwait", "科威特", "KW", "KWT", "965"),
            new CountryInfo("Kyrgyzstan", "吉尔吉斯斯坦", "KG", "KGZ", "996"),
            new CountryInfo("Lao People's Democratic Republic", "老挝", "LA", "LAO", "856"),
            new CountryInfo("Lebanon", "黎巴嫩", "LB", "LBN", "961"),
            new CountryInfo("Macao", "中国澳门特别行政区", "MO", "MAC", "853"),
            new CountryInfo("Malaysia", "马来西亚", "MY", "MYS", "60"),
            new CountryInfo("Maldives", "马尔代夫", "MV", "MDV", "960"),
            new CountryInfo("Mongolia", "蒙古", "MN", "MNG", "976"),
            new CountryInfo("Myanmar", "缅甸", "MM", "MMR", "95"),
            new CountryInfo("Nepal", "尼泊尔", "NP", "NPL", "977"),
            new CountryInfo("Oman", "阿曼", "OM", "OMN", "968"),
            new CountryInfo("Pakistan", "巴基斯坦", "PK", "PAK", "92"),
            new CountryInfo("Palestinian Territory", "巴勒斯坦", "PS", "PSE", "970"),
            new CountryInfo("Philippines", "菲律宾", "PH", "PHL", "63"),
            new CountryInfo("Qatar", "卡塔尔", "QA", "QAT", "974"),
            new CountryInfo("Saudi Arabia", "沙特阿拉伯", "SA", "SAU", "966"),
            new CountryInfo("Singapore", "新加坡", "SG", "SGP", "65"),
            new CountryInfo("Sri Lanka", "斯里兰卡", "LK", "LKA", "94"),
            new CountryInfo("Syrian Arab Republic", "叙利亚", "SY", "SYR", "963"),
            new CountryInfo("Taiwan, Province of China", "中国台湾", "TW", "TWN", "886"),
            new CountryInfo("Tajikistan", "塔吉克斯坦", "TJ", "TJK", "992"),
            new CountryInfo("Thailand", "泰国", "TH", "THA", "66"),
            new CountryInfo("Timor-Leste", "东帝汶", "TL", "TLS", "670"),
            new CountryInfo("Turkmenistan", "土库曼斯坦", "TM", "TKM", "993"),
            new CountryInfo("United Arab Emirates", "阿拉伯联合酋长国", "AE", "ARE", "971"),
            new CountryInfo("Uzbekistan", "乌兹别克斯坦", "UZ", "UZB", "998"),
            new CountryInfo("Vietnam", "越南", "VN", "VNM", "84"),
            new CountryInfo("Yemen", "也门", "YE", "YEM", "967"),
        };
    }
}