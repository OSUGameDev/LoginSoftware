using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace GameDevLogin
{
    [Serializable]
    public class SpreadSheetInfo : INotifyPropertyChanged, ISerializable
    {
        public SpreadSheetInfo()
        {
            sheetId = "";
        }

        /// <summary>
        /// Id of the spread sheet
        /// </summary>
        public string SheetId{ 
            get => sheetId;
            set {
                sheetId = value;
                RaisePropertyChanged(nameof(SheetId));
            }
        }

        /// <summary>
        /// Backend field for the <see cref="SheetId"/> property
        /// </summary>
        private string sheetId;

        #region INotifyProperty
        /// <summary>
        /// Event to be called whenever the objects properties changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the property changed event
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Serializes the data for the object
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(SheetId), SheetId, typeof(string));
        }

        /// <summary>
        /// Create the object from a serialized state
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SpreadSheetInfo(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            sheetId = (string)info.GetValue(nameof(SheetId), typeof(string));
        }
        #endregion Serialization
    }
}
