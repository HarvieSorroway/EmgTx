using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[BepInPlugin("emgtx", "EmgTx", "1.0.0")]
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
