﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    private GameWorld gw;
    private int rand;
    public GameObject spawner;
    public bool doStuff = false;
    
    [SyncVar]
    public int disconnected = 0;

    public GameObject opponentPlayer;
    public bool requestedRematch = false;
    public bool acceptedRematch = false;
    bool changedText = false;

    void Start() {
        gw = FindObjectOfType<GameWorld>().GetComponent<GameWorld>();

        if (!isLocalPlayer) return;
        if (!isServer) {
            gw.client = true;
            return;
        }
        CmdRand();
    }

    void Update()
    {
        if (doStuff)
        {
            if (isLocalPlayer) {
                doStuff = false;
                CmdDostuff();
            }
        }

        if (isServer) {
            if (opponentPlayer) {
                if (disconnected == 0 && opponentPlayer.GetComponent<Player>().disconnected == 1)
                {
                    disconnected = 1;
                    GameObject.Find("Disconnect Button").GetComponent<Disconnect>().startChecking = true;
                    GameObject.Find("Disconnect Button").GetComponent<Disconnect>().networkManager = NetworkManager.singleton;
                }
            }
            else
            {
                TargetOpponent();
            }
        }

        if (opponentPlayer != null)
        {
            if (opponentPlayer.GetComponent<Player>().requestedRematch && !changedText)
            {
                changedText = true;
                GameObject.FindGameObjectWithTag("EndText").GetComponent<Text>().text = "ACCEPT?";
            }
            if (requestedRematch && opponentPlayer.GetComponent<Player>().acceptedRematch)
            {
                requestedRematch = false;
                changedText = false;
                gw.GetComponent<GameWorld>().Restart();
            }
            else if(acceptedRematch){
                acceptedRematch = false;
                changedText = false;
                gw.GetComponent<GameWorld>().Restart();
            }
        }
    }

    public void DoRematch()
    {
        if (opponentPlayer == null)
        {
            opponentPlayer = GameObject.Find("Player(Clone)");
        }
        if (!opponentPlayer.GetComponent<Player>().requestedRematch)
        {
            GameObject.FindGameObjectWithTag("EndText").GetComponent<Text>().text = "WAITING";
            CmdRequestRematch();
        }
        else
        {
            CmdAcceptRematch();
        }
    }

    public bool CheckIfServer()
    {
        return isServer;
    }

    public void EndForReal()
    {
        if (isServer)
        {
            GameObject.Find("Disconnect Button").GetComponent<Disconnect>().ReallyEnd();
        }
    }

    [Command]
    public void CmdRand() {
        for (int i = 0; i < Random.Range(3, 6); i++) {
            rand = Random.Range(0, 25);
            while (!gw.buttonList[rand].interactable) rand = Random.Range(0, 25);
            gw.syncList[rand] = rand;

        }
    }

    [Command]
    public void CmdCheckSquares(string tag) {
        RpcCheckSquares(tag);
    }

    [ClientRpc]
    void RpcCheckSquares(string tag) {
        gw.CheckSquares(tag);
    }

    [Command]
    public void CmdEndTurn() {
        RpcEndTurn();
    }

    [ClientRpc]
    public void RpcEndTurn() {
        gw.EndTurn();
    }

    [Command]
    public void CmdDeleteInMove(string tag, float thisPos, float lastPos) {
        RpcDeleteInMove(tag, thisPos, lastPos);
    }

    [ClientRpc]
    public void RpcDeleteInMove(string tag, float thisPos, float lastPos) {
        gw.buttonList[int.Parse(tag)].GetComponent<Square>().DeleteInMove(thisPos, lastPos);
    }

    [Command]
    public void CmdRequestRematch()
    {
        RpcRequestRematch();
    }

    [ClientRpc]
    public void RpcRequestRematch()
    {
        requestedRematch = true;
    }

    [Command]
    public void CmdAcceptRematch()
    {
        RpcAcceptRematch();
    }

    [ClientRpc]
    public void RpcAcceptRematch()
    {
        acceptedRematch = true;
    }

    [Command]
    public void CmdCancelMove() {
        RpcCancelMove();
    }

    [ClientRpc]
    public void RpcCancelMove() {
        gw.CancelMove();
    }

    [Command]
    public void CmdUnSelect() {
        RpcUnSelect();
    }

    [ClientRpc]
    public void RpcUnSelect() {
        gw.UnSelect();
    }

    [Command]
    void CmdDostuff()
    {
        disconnected = 1;
    }

    public override void OnStartLocalPlayer()
    {
        gameObject.name="localPlayer";
    }

    public void TargetOpponent()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 2)
        {
            opponentPlayer = GameObject.Find("Player(Clone)");
            opponentPlayer.GetComponent<Player>().opponentPlayer = GameObject.Find("localPlayer");
        }
    }
}