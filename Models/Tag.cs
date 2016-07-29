using System;
using System.Runtime.InteropServices;

namespace EasyOPC
{
    [ComVisible(true)]
    [Guid("C13AC408-FE55-4CF7-BC7D-97F414083C5D")]
    public class ETag : ITag
    {
        private string _displayName;
        private string _tagID;
        private string _tagIdentifier;
        private object _value;
        private int _quality;
        private DateTime _timeStamp;
        private Type _type;

        internal ETag(string tagID,string tagIdentifier)
        {
            _tagIdentifier = tagIdentifier;
            _tagID = tagID;
        }

        internal ETag(string tagID,string tagIdentifier,string displayName) :this(tagID, tagIdentifier)
        {
            _displayName = displayName;
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }            
        }

        public string TagID
        {
            get
            {
                return _tagID;
            }
        }

        public string TagIdentifier
        {
            get
            {
                return _tagIdentifier;
            }
        }

        public object Value
        {
            get
            {
                return _value;   
            }
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
        }

        public int Quality
        {
            get { return _quality; }
        }

        public Type DataType
        {
            get { return _type; }
        }

        public void SetValue(object value)
        {
            SetValue(value, 0);
        }

        public void SetValue(object value,int quality)
        {
            _value = value;            
            _quality = quality;

            if (value != null)
                _type = value.GetType();
        }

        public void SetValue(object value, int quality, DateTime timeStamp)
        {
            SetValue(value, quality);
            _timeStamp = timeStamp;
        }
    }
}
