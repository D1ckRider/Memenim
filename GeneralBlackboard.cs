﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnonymDesktopClient
{
    enum BlackBoardValues
    {
        EPageToRedirect,
        EPostData,
        EBackPage
    }

    static class GeneralBlackboard
    {
        static private Dictionary<BlackBoardValues, object> m_boardValues;

        static GeneralBlackboard()
        {
            m_boardValues = new Dictionary<BlackBoardValues, object>();
        }

        public static object TryGetValue(BlackBoardValues key)
        {
            object value;
            m_boardValues.TryGetValue(key, out value);
            return value;
        }

        public static T TryGetValue<T>(BlackBoardValues key)
        {
            object value;
            m_boardValues.TryGetValue(key, out value);
            return (T)value;
        }


        public static void SetValue(BlackBoardValues key, object value)
        {
            if(!m_boardValues.ContainsKey(key))
            {
                m_boardValues.Add(key, value);
            }
            else
            {
                m_boardValues[key] = value;
            }
        }
    }
}
