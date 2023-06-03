using System.Collections.Generic;

namespace GeneralSQLAPI
{
    public class R<T>
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public T Result { get; set; }
        private Dictionary<string, object> Map { get; set; }

        public static R<T> Success(T Result)
        {
            R<T> r = new R<T>();
            r.Result = Result;
            r.Msg = "OK";
            r.Code = "0000";
            return r;
        }

        public static R<T> Error(string msg)
        {
            R<T> r = new R<T>();
            r.Msg = msg;
            r.Code = "1000";
            return r;
        }

        public R<T> Add(string key, object value)
        {
            if (Map == null)

                Map = new Dictionary<string, object>();

            Map.Add(key, value);
            return this;
        }
    }
}