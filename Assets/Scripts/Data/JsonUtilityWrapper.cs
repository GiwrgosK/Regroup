using UnityEngine;
using System;
using System.Collections.Generic;

public static class JsonUtilityWrapper {
    public static List<T> FromJsonList<T>(string json) {
        return JsonUtility.FromJson<Wrapper<T>>("{\"Items\":" + json + "}").Items;
    }

    [Serializable] private class Wrapper<T> {
        public List<T> Items;
    }
}