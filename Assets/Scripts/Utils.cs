using System;
using System.Collections.Generic;

namespace RandomListSelection{
    public static class ListExtensions{
        public static T Random<T>(this List<T> list){
            Random rand = new Random();
            return list[rand.Next(list.Count)];
        }
    }
}