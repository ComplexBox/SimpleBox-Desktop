// https://github.com/crifan/crifanLib/blob/master/csharp/crifanLib.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleBox.Utils.Cef
{
    public struct PairItem
    {
        public string Key;
        public string Value;
    }

    public sealed class CookieHelper
    {
        #region Current

        public static CookieHelper Current { get; } = new CookieHelper();

        #endregion

        #region Const Data

        private const char ReplacedChar = '_';
        private const string StrExpires = "expires";
        private readonly string[] CookieFieldArray = { StrExpires, "domain", "secure", "path", "httponly", "version" };

        #endregion

        #region Core Data

        private List<string> _cookieFieldList = new List<string>();

        #endregion

        #region Constructor

        public CookieHelper()
        {
            ServicePointManager.DefaultConnectionLimit = 10000;

            foreach (string key in CookieFieldArray) _cookieFieldList.Add(key);
        }

        #endregion

        #region Methods

        private bool NeedAddThisCookie(Cookie ck, string curDomain)
        {
            bool needAdd;

            if (ck == null || ck.Name == "")
                needAdd = false;
            else
            {
                if (ck.Domain != "")
                    needAdd = true;
                else if (curDomain != "")
                {
                    ck.Domain = curDomain;
                    needAdd = true;
                }
                else
                    needAdd = false;
            }

            return needAdd;
        }

        private string ExtractHost(string url)
        {
            string domain = "";
            if (url == "" || !url.Contains("/")) return domain;
            string[] split = url.Split('/');
            domain = split[2];
            return domain;
        }

        public string GetDomainUrl(string url)
        {
            string domainUrl = "";

            Regex urlRx = new Regex(@"((https)|(http)|(ftp))://[\w\-\.]+");
            Match foundUrl = urlRx.Match(url);
            domainUrl = foundUrl.Success ? url.Substring(0, foundUrl.Length) : "";

            return domainUrl;
        }

        private bool AddFieldToCookie(ref Cookie ck, PairItem pairInfo)
        {
            bool added = false;
            if (pairInfo.Key == "") return false;
            string lowerKey = pairInfo.Key.ToLower();
            switch (lowerKey)
            {
                case "expires":
                    bool parseDatetimeOk;
                    parseDatetimeOk = DateTime.TryParse(pairInfo.Value, out DateTime expireDatetime);
                    if (parseDatetimeOk)
                    {
                        ck.Expires = expireDatetime;

                        if (DateTime.Now.Ticks > ck.Expires.Ticks) ck.Expired = true;

                        added = true;
                    }
                    break;
                case "domain":
                    ck.Domain = pairInfo.Value;
                    added = true;
                    break;
                case "secure":
                    ck.Secure = true;
                    added = true;
                    break;
                case "path":
                    ck.Path = pairInfo.Value;
                    added = true;
                    break;
                case "httponly":
                    ck.HttpOnly = true;
                    added = true;
                    break;
                case "version":
                    if (int.TryParse(pairInfo.Value, out int versionValue))
                    {
                        ck.Version = versionValue;
                        added = true;
                    }
                    break;
            }

            return added;
        }

        private bool IsValidCookieField(string cookieKey) => _cookieFieldList.Contains(cookieKey.ToLower());

        private bool IsValidCookieName(string ckName)
        {
            bool isValid = true;
            if (ckName == null)
                isValid = false;
            else
            {
                string invalidP = @"\W+";
                Regex rx = new Regex(invalidP);
                Match foundInvalid = rx.Match(ckName);
                if (foundInvalid.Success) isValid = false;
            }

            return isValid;
        }

        private bool ParseCookieNameValue(string ckNameValueExpr, out PairItem pair)
        {
            bool parsedOK = false;
            if (ckNameValueExpr == "")
            {
                pair.Key = "";
                pair.Value = "";
                parsedOK = false;
            }
            else
            {
                ckNameValueExpr = ckNameValueExpr.Trim();

                int equalPos = ckNameValueExpr.IndexOf('=');
                if (equalPos > 0) // is valid expression
                {
                    pair.Key = ckNameValueExpr.Substring(0, equalPos);
                    pair.Key = pair.Key.Trim();
                    if (IsValidCookieName(pair.Key))
                    {
                        // only process while is valid cookie field
                        pair.Value = ckNameValueExpr.Substring(equalPos + 1);
                        pair.Value = pair.Value.Trim();
                        parsedOK = true;
                    }
                    else
                    {
                        pair.Key = "";
                        pair.Value = "";
                        parsedOK = false;
                    }
                }
                else
                {
                    pair.Key = "";
                    pair.Value = "";
                    parsedOK = false;
                }
            }
            return parsedOK;
        }

        private bool ParseCookieField(string ckFieldExpr, out PairItem pair)
        {
            bool parsedOK = false;

            if (ckFieldExpr == "")
            {
                pair.Key = "";
                pair.Value = "";
                parsedOK = false;
            }
            else
            {
                ckFieldExpr = ckFieldExpr.Trim();

                switch (ckFieldExpr.ToLower())
                {
                    case "httponly":
                        pair.Key = "httponly";
                        //pair.value = "";
                        pair.Value = "true";
                        parsedOK = true;
                        break;
                    case "secure":
                        pair.Key = "secure";
                        //pair.value = "";
                        pair.Value = "true";
                        parsedOK = true;
                        break;
                    // normal cookie field
                    default:
                    {
                        int equalPos = ckFieldExpr.IndexOf('=');
                        if (equalPos > 0)
                        {
                            pair.Key = ckFieldExpr.Substring(0, equalPos);
                            pair.Key = pair.Key.Trim();
                            if (IsValidCookieField(pair.Key))
                            {
                                pair.Value = ckFieldExpr.Substring(equalPos + 1);
                                pair.Value = pair.Value.Trim();
                                parsedOK = true;
                            }
                            else
                            {
                                pair.Key = "";
                                pair.Value = "";
                                parsedOK = false;
                            }
                        }
                        else
                        {
                            pair.Key = "";
                            pair.Value = "";
                            parsedOK = false;
                        }

                        break;
                    }
                }
            }

            return parsedOK;
        }

        private string[] GetSubStrArr(string[] origStrArr, int startIdx, int len)
        {
            string[] subStrArr = { };
            if (origStrArr == null || origStrArr.Length <= 0 || len <= 0) return subStrArr;
            List<string> strList = new List<string>();
            int endPos = startIdx + len;
            if (endPos > origStrArr.Length) endPos = origStrArr.Length;

            for (int i = startIdx; i < endPos; i++) strList.Add(origStrArr[i]);

            subStrArr = new string[len];
            strList.CopyTo(subStrArr);

            return subStrArr;
        }

        private bool ParseSingleCookie(string cookieStr, ref Cookie ck)
        {
            bool parsedOk = true;

            string[] expressions = cookieStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            PairItem pair = new PairItem();
            if (ParseCookieNameValue(expressions[0], out pair))
            {
                ck.Name = pair.Key;
                ck.Value = pair.Value;

                string[] fieldExpressions = GetSubStrArr(expressions, 1, expressions.Length - 1);
                bool noDeisignateExpires = true;
                foreach (string eachExpression in fieldExpressions)
                {
                    //parse key and value
                    if (ParseCookieField(eachExpression, out pair))
                    {
                        // add to cookie field if possible
                        bool addedOk = false;
                        addedOk = AddFieldToCookie(ref ck, pair);
                        if (addedOk && string.Equals(pair.Key, StrExpires))
                        {
                            noDeisignateExpires = false;
                        }
                    }
                    else
                    {
                        parsedOk = false;
                        break;
                    }
                }
                if (noDeisignateExpires) ck.Expires = DateTime.MaxValue;
            }
            else
                parsedOk = false;

            return parsedOk;
        }

        private static string RecoverExpireField(Match foundProcessedExpire) =>
            foundProcessedExpire.Value.Replace(ReplacedChar, ',');

        private static string ProcessExpireField(Match foundExpire) => foundExpire.Value.Replace(',', ReplacedChar);

        private CookieCollection ParseSetCookie(string setCookieStr, string curDomain)
        {
            CookieCollection parsedCookies = new CookieCollection();

            if (string.IsNullOrEmpty(setCookieStr)) return parsedCookies;

            string commaReplaced = Regex.Replace(setCookieStr, @"xpires=\w{3},\s\d{2}-\w{3}-\d{2,4}", ProcessExpireField);
            string[] cookieStrArr = commaReplaced.Split(',');
            foreach (string cookieStr in cookieStrArr)
            {
                Cookie ck = new Cookie();

                string recoveredCookieStr = Regex.Replace(cookieStr, @"xpires=\w{3}" + ReplacedChar + @"\s\d{2}-\w{3}-\d{2,4}", RecoverExpireField);
                if (!ParseSingleCookie(recoveredCookieStr, ref ck)) continue;
                if (NeedAddThisCookie(ck, curDomain)) parsedCookies.Add(ck);
            }

            return parsedCookies;
        }

        public CookieCollection ParseSetCookie(string setCookieStr)
        {
            return ParseSetCookie(setCookieStr, "");
        }

        private static bool ExtractSingleStr(string pattern, string extractFrom, out string extractedStr, RegexOptions regexOption = RegexOptions.None)
        {
            bool extractOk;
            Regex rx = new Regex(pattern, regexOption);
            Match found = rx.Match(extractFrom);
            if (found.Success)
            {
                extractOk = true;
                extractedStr = found.Groups[1].ToString();
            }
            else
            {
                extractOk = false;
                extractedStr = "";
            }

            return extractOk;
        }

        private static DateTime MillisecondToDateTime(double millisecondSinceEpoch)
        {
            DateTime st = new DateTime(1970, 1, 1, 0, 0, 0);
            st = st.AddMilliseconds(millisecondSinceEpoch);
            return st;
        }

        private static bool ParseJsNewDate(string newDateStr, out DateTime parsedDatetime)
        {
            bool parseOk = false;
            parsedDatetime = new DateTime();

            if (newDateStr == "" || newDateStr.Trim() == "") return false;

            if (!ExtractSingleStr(@".*new\sDate\((.+?)\).*", newDateStr, out string dateValue)) return parseOk;

            if (double.TryParse(dateValue, out double doubleVal))
            {
                parsedDatetime = MillisecondToDateTime(doubleVal);
                parseOk = true;
            }
            else if (DateTime.TryParse(dateValue, out parsedDatetime)) parseOk = true;

            return parseOk;
        }

        public bool ParseJsSetCookie(string singleSetCookieStr, out Cookie parsedCk)
        {
            parsedCk = new Cookie();

            string setckP = @"\$Cookie\.setCookie\('(\w+)',\s*'(.*?)',\s*'([\w\.]+)',\s*'(.+?)',\s*(.+?),\s*(\d?)\);";
            Regex setckRx = new Regex(setckP);
            Match foundSetck = setckRx.Match(singleSetCookieStr);

            if (!foundSetck.Success) return false;

            string name = foundSetck.Groups[1].ToString();
            string value = foundSetck.Groups[2].ToString();
            string domain = foundSetck.Groups[3].ToString();
            string path = foundSetck.Groups[4].ToString();
            string expire = foundSetck.Groups[5].ToString();
            string secure = foundSetck.Groups[6].ToString();

            if (!IsValidCookieName(name) || domain == "") return false;

            parsedCk.Name = name;
            parsedCk.Value = value;
            parsedCk.Domain = domain;
            parsedCk.Path = path;

            if (expire.Trim() != "null")
            {
                DateTime expireTime;
                if (ParseJsNewDate(expire, out expireTime)) parsedCk.Expires = expireTime;
            }

            parsedCk.Secure = secure == "1";

            return true;
        }

        public bool IsCookieExpired(Cookie ck)
        {
            bool isExpired = false;

            if (ck != null && ck.Name != "")
            {
                if (!ck.Expired)
                {
                    DateTime initExpiresValue = new Cookie().Expires;
                    DateTime expires = ck.Expires;

                    if (expires.Equals(initExpiresValue)) return false;
                    if (DateTime.Now.Ticks > expires.Ticks) isExpired = true;
                }
                else
                    isExpired = true;
            }
            else
                isExpired = true;

            return isExpired;
        }

        private static void AddCookieToCookies(Cookie toAdd, ref CookieCollection cookies, bool overwriteDomain)
        {
            bool found = false;

            if (cookies.Count > 0)
            {
                foreach (Cookie originalCookie in cookies)
                {
                    if (originalCookie.Name != toAdd.Name) continue;
                    if (originalCookie.Domain != toAdd.Domain &&
                        (originalCookie.Domain == toAdd.Domain || !overwriteDomain)) continue;
                    originalCookie.Value = toAdd.Value;

                    originalCookie.Domain = toAdd.Domain;

                    originalCookie.Expires = toAdd.Expires;
                    originalCookie.Version = toAdd.Version;
                    originalCookie.Path = toAdd.Path;

                    found = true;
                    break;
                }
            }

            if (found) return;
            if (toAdd.Domain != "") cookies.Add(toAdd);

        }

        private static void AddCookieToCookies(Cookie toAdd, ref CookieCollection cookies) =>
            AddCookieToCookies(toAdd, ref cookies, false);

        private static bool IsContainCookie(object ckToCheck, object cookies)
        {
            bool isContain = false;

            if (ckToCheck == null || cookies == null) return false;
            string ckName = "";
            Type type = ckToCheck.GetType();

            if (type.Name.ToLower() == "string")
                ckName = (string) ckToCheck;
            else if (type.Name == "Cookie") ckName = ((Cookie) ckToCheck).Name;

            if (ckName == "") return false;

            type = cookies.GetType();

            switch (type.Name)
            {
                case "Cookie":
                    if (ckName == ((Cookie) cookies).Name) isContain = true;

                    break;
                case "CookieCollection":
                    if (((CookieCollection) cookies).Cast<Cookie>().Any(ck => ckName == ck.Name)) isContain = true;

                    break;
                default:
                    switch (type.Name.ToLower())
                    {
                        case "string":
                            if (ckName == (string) cookies) isContain = true;

                            break;
                        case "string[]":
                            if (((string[]) cookies).Any(name => ckName == name)) isContain = true;

                            break;
                    }

                    break;
            }

            return isContain;
        }

        private static void UpdateLocalCookies(CookieCollection cookiesToUpdate, ref CookieCollection localCookies, object omitUpdateCookies)
        {
            if (cookiesToUpdate.Count <= 0) return;
            if (localCookies == null)
                localCookies = cookiesToUpdate;
            else
                foreach (Cookie newCookie in cookiesToUpdate)
                    if (!IsContainCookie(newCookie, omitUpdateCookies))
                        AddCookieToCookies(newCookie, ref localCookies);
        }

        public void UpdateLocalCookies(CookieCollection cookiesToUpdate, ref CookieCollection localCookies) =>
            UpdateLocalCookies(cookiesToUpdate, ref localCookies, null);

        public bool GetCookieVal(string ckName, ref CookieCollection cookies, out string ckVal)
        {
            ckVal = "";
            bool gotValue = false;

            foreach (Cookie ck in cookies)
            {
                if (ck.Name == ckName)
                {
                    gotValue = true;
                    ckVal = ck.Value;
                    break;
                }
            }

            return gotValue;
        }

        #endregion
    }
}
