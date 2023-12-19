namespace jmayberry.SerializableDictionary {
    //public partial class SerializableDictionary<TKey, TValue> : SerializedDictionary<TKey, TValue>, ISerializationCallbackReceiver {
    //    public void IncrementValue(TKey key, TValue amount) {
    //        if (!this.ContainsKey(key)) {
    //            this[key] = default;
    //        }

    //        switch (this[key]) {
    //            case int intValue when amount is int intAmount:
    //                this[key] = (TValue)(object)(intValue + intAmount);
    //                break;

    //            case float floatValue when amount is float floatAmount:
    //                this[key] = (TValue)(object)(floatValue + floatAmount);
    //                break;

    //            case double doubleValue when amount is double doubleAmount:
    //                this[key] = (TValue)(object)(doubleValue + doubleAmount);
    //                break;

    //            case long longValue when amount is long longAmount:
    //                this[key] = (TValue)(object)(longValue + longAmount);
    //                break;

    //            case decimal decimalValue when amount is decimal decimalAmount:
    //                this[key] = (TValue)(object)(decimalValue + decimalAmount);
    //                break;

    //            default:
    //                Debug.LogError($"Type of TValue is not supported for IncrementValue operation.");
    //                break;
    //        }
    //    }
    //}
}
