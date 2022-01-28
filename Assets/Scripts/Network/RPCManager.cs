using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPCManager
{

    public static void SurvivorStartRPC(GameObject _gameObject, string _rpcName, Vector3 pos1, Vector3 pos2)
    {
        _gameObject.GetComponentInChildren<SurvivorN>().StartRPC(_rpcName, pos1, pos2);
    }

    public static void SurvivorStartRPC(GameObject _gameObject,string _rpcName)
    {
        _gameObject.GetComponentInChildren<SurvivorN>().StartRPC(_rpcName);
    }

    public static void SurvivorStartRPCString(GameObject _gameObject,string _rpcName,string _tag)
    {
        _gameObject.GetComponentInChildren<SurvivorN>().StartRPCString(_rpcName, _tag);
    }

    public static void BoardStartRPC(GameObject _gameObject,string _rpcName)
    {
        _gameObject.GetComponentInChildren<Board>().StartBoardRPC(_rpcName);
    }

    public static void BoardRangeStartRPC(GameObject _gameObject,string _rpcName)
    {
        _gameObject.GetComponentInChildren<BoardRange>().StartBoardRangeRPC(_rpcName);
    }

    public static void GeneratorGaugeUpRpc(Generator _currentGen, float _repairPower)
    {
        _currentGen.GaugeUpRpcCall(_repairPower);
    }

    public static void BrokenGeneratorRpc(Generator _currentGen,float _power)
    {
        _currentGen.BrokenGenRPC(_power);
    }

    public static void HangerStartRPC(GameObject _gameObject, string _rpcName)
    {
        _gameObject.GetComponent<HangerScr_>().StartRPC(_rpcName);
    }


}
