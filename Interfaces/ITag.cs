using System;
using System.Runtime.InteropServices;

namespace EasyOPC
{
    /// <summary>
    /// OPC Tag
    /// </summary>
    [ComVisible(true)]
    [Guid("174D20D1-EFE9-40B9-8546-9055D68B2542")]
    public interface ITag
    {
        /// <summary>
        /// Display name of the tag
        /// </summary>
        String DisplayName { get; }

        /// <summary>
        /// ID of the tag
        /// </summary>
        string TagID { get; }
        
        /// <summary>
        /// Tag identified(full path of the tag)
        /// </summary>
        string TagIdentifier { get; }

        /// <summary>
        /// The value of the tag
        /// </summary>
        object Value { get;}               

        /// <summary>
        /// Quality of the last read
        /// </summary>
        int Quality { get; }

        /// <summary>
        /// The Type of data
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Sets the Value property of the tag
        /// </summary>
        /// <param name="value">Value</param>
        void SetValue(object value);

        /// <summary>
        /// DateTime - last read value
        /// </summary>
        DateTime TimeStamp { get; }

        /// <summary>
        /// Sets the Value property of the tag
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="quality">Quality</param>
        void SetValue(object value, int quality);

        /// <summary>
        /// Sets the value,quality and timestamp for a tag
        /// </summary>
        /// <param name="value"></param>
        /// <param name="quality"></param>
        /// <param name="timeStamp"></param>
        void SetValue(object value, int quality,DateTime timeStamp);
    }
}
