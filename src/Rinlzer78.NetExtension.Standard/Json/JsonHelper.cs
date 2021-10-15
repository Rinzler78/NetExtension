using System;

namespace Rinlzer78.NetExtension.Json
{
    public static class JsonHelper
    {
        public static string SerializeObject(this object obj) => Newtonsoft.Json.JsonConvert.SerializeObject(obj);
    }
}
