using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EmgTxCustom
{
    public static void Log(string msg)
    {
        Debug.Log("[EmgTx]" + msg);
    }

    public static void Log(string pattern,params object[] vars)
    {
        Debug.Log("[EmgTx]" + string.Format(pattern, vars));
    }
}
