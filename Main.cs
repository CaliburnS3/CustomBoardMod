using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using MelonLoader;
using RidersX;
using System.IO;
using System.Reflection;

//Code by Riz#0119

namespace CustomBoards
{
    public class BoardMod : MelonMod
    {
        AssetLoad AssetMain = new AssetLoad();
        RidersX.FX.BetterTrail trailName;
        int trailRef = 0;
        GameObject bActivate = null;
        GameObject bDeactivate = null;
        GameObject board;


        //delay on scene load counter
        int[] counter = { 1, 0 };
        //This is the name of the board to search and replace
        public string[,] boardNames = new string[1, 2];

        int sceneIndex = 0;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
        }


        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //This resets the board override timer on a scene load
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonLogger.Msg(buildIndex + ":" + sceneName);
            counter[1] = 0;
            board = null;
            sceneIndex = buildIndex;
        }

        public override void OnUpdate()
        {

            if (counter[0] == counter[1])
            {
                counter[1]++;
                //Update read paths for assetbundles then load assetbundle if match found in files with game
                boardNames = AssetMain.fileRead();
                for (int i = 0; i < boardNames.GetLength(0); i++)
                {
                    board = GameObject.Find(boardNames[i, 1]);
                    if (board != null)
                    {
                        AssetMain.StageBoardLoadMethod(boardNames[i, 0], boardNames[i, 1]);
                    }
                }
                if (GameObject.Find("Trail").GetComponent<RidersX.FX.BetterTrail>())
                {
                    trailName = GameObject.Find("Trail").GetComponent<RidersX.FX.BetterTrail>();
                    trailRef = 1;

                    if (GameObject.Find("BoardActivate"))
                    {
                        bActivate = GameObject.Find("BoardActivate");
                        bActivate.SetActive(false);
                    }


                    if (GameObject.Find("BoardDeactivate"))
                    {
                        bDeactivate = GameObject.Find("BoardDeactivate");
                        bDeactivate.SetActive(true);
                    }
                }

            }


            if (trailRef == 1)
            {
                if (trailName.enabled == true)
                {
                    //this should trigger when starting race\
                    trailRef = 0;
                    bActivate.SetActive(true);
                    bDeactivate.SetActive(false);

                }

            }





            if (counter[0] > counter[1])
            {
                counter[1]++;
            }
            if (Input.GetKeyDown("x"))
            {
                AssetMain.printFunction();
            }
            //changes values to allow for menu board to change again
            if (sceneIndex == 0 && GameObject.Find("2D Gear"))
            {
                sceneIndex = 1;
            }

            //Replaces board shown in menu
            if (sceneIndex == 1 && GameObject.Find("Attachment_Board"))
            {
                sceneIndex = 0;
                AssetMain.MenuBoardLoadMethod();
            }
        }
    }

    public class AssetLoad : MonoBehaviour
    {
        public void printFunction()
        {
            //Print all game objects in scene
            Renderer[] tester = FindObjectsOfType<Renderer>();
            for (int i = 0; i < tester.Length; i++)
            {
                MelonLogger.Msg(tester[i].gameObject.name);
            }
        }

        public void MenuBoardLoadMethod()
        {
            //This mimics StageBoardLoadFunction, however this works for the menu only
            GameObject menuTemp = GameObject.Find("Attachment_Board");
            string trim = menuTemp.transform.GetChild(0).name;
            string[] results = trim.Split('(');
            if (results[0] == "TypeJ")
            {
                results[0] = "Type-J";
            }
            GameObject BoardTemp = AssetLoadMethod(results[0]);
            GameObject[] temp = { null, null };
            temp[1] = GameObject.Find(trim);
            temp[0] = Instantiate(BoardTemp);
            MelonLogger.Msg("Instantiate: " + trim);
            temp[0].transform.parent = temp[1].transform.parent;
            temp[0].transform.localPosition = Vector3.zero;
            temp[0].transform.rotation = temp[1].transform.rotation;
            //Destroy(temp[1].gameObject);
            temp[1].SetActive(false);
        }

        public string[,] fileRead()
        {
            //OnSceneLoad, get all custom board names 
            string codeFilePath = Assembly.GetExecutingAssembly().Location;
            codeFilePath = Path.GetDirectoryName(codeFilePath);
            codeFilePath += @"\Resources\";
            string[] fileArray = Directory.GetFiles(codeFilePath);
            string[,] boardNames = new string[fileArray.GetLength(0), 2];
            for (int i = 0; i < fileArray.GetLength(0); i++)
            {
                //split string to get just board name, then build with board names
                fileArray[i] = Path.GetFileName(fileArray[i]);
                boardNames[i, 0] = fileArray[i];
                boardNames[i, 1] = fileArray[i] + "(Clone)";
            }
            return boardNames;
        }

        public GameObject AssetLoadMethod(string boardName_0)
        {
            //Load file from resource folder with the name given by boardName_0
            string codeFilePath = Assembly.GetExecutingAssembly().Location;
            codeFilePath = Path.GetDirectoryName(codeFilePath);
            codeFilePath = codeFilePath + @"\Resources\" + boardName_0;
            MelonLogger.Msg("--------------------------------------");
            MelonLogger.Msg("Path: " + codeFilePath);

            //Load resource into the game
            AssetBundle assetBundle = AssetBundle.LoadFromFile(codeFilePath);
            MelonLogger.Msg("Asset loaded: " + assetBundle.name);

            //Initialize Board
            string[] assetNames = assetBundle.GetAllAssetNames();
            GameObject BoardTemp = assetBundle.LoadAsset(assetNames[0]).Cast<GameObject>();
            assetBundle.Unload(false);
            return BoardTemp;
        }

        public void StageBoardLoadMethod(string boardName_0, string boardName_1)
        {
            //Goodbye Camera Box
            GameObject destroyed = GameObject.Find("PhotoModeBounds");
            destroyed.transform.localScale += new Vector3(10.1f, 10.1f, 10.1f);

            GameObject BoardTemp = AssetLoadMethod(boardName_0);
            //customBoard, startBoard
            //temp[0], temp[1]
            GameObject[] boards = { null, null };
            boards[1] = GameObject.Find(boardName_1);
            boards[0] = Instantiate(BoardTemp);
            MelonLogger.Msg("Instantiate: " + boardName_1);

            //Setup for following features
            Transform[] startBoardChildren = boards[1].transform.GetComponentsInChildren<Transform>();
            Transform[] customBoardChildren = boards[0].transform.GetComponentsInChildren<Transform>();
            Transform[] childrenHolder = { null, null, null };

            //Finds the TrailOrigin, save in childrenHolder for later
            for (int ii = 1; ii < customBoardChildren.Length; ii++)
            {
                if (customBoardChildren[ii].name == "TrailOrigin")
                {
                    childrenHolder[0] = customBoardChildren[ii].transform;
                }
            }

            //StartingBoard children manipulation, only if TrailOrigin exists
            if (childrenHolder[0] != null)
            {
                for (int i = 0; i < startBoardChildren.Length; i++)
                {
                    if (startBoardChildren[i].name.Contains("Trail"))
                    {
                        childrenHolder[1] = startBoardChildren[i].transform;
                        childrenHolder[1].transform.rotation = childrenHolder[0].transform.rotation;
                        childrenHolder[1].transform.localScale = childrenHolder[0].transform.localScale;
                    }
                    if (startBoardChildren[i].name.Contains("Charge Jump"))
                    {
                        childrenHolder[2] = startBoardChildren[i].transform;
                        childrenHolder[2].transform.localScale = childrenHolder[0].transform.localScale;
                    }
                }

                //manipulated children transfered over 
                for (int i = 1; i < childrenHolder.Length; i++)
                {
                    GameObject tempTrail = childrenHolder[i].gameObject;
                    tempTrail.transform.parent = childrenHolder[0];
                    tempTrail.transform.localPosition = childrenHolder[0].localPosition;
                    tempTrail.transform.localScale = childrenHolder[i].localScale;
                }
            }

            //Removes the now obsolete startBoard mesh
            Destroy(startBoardChildren[1].gameObject);

            //Syncs custom board to proper rotation, position
            //Puts customBoard where startBoard would have been
            boards[0].transform.parent = boards[1].transform;
            boards[0].transform.localPosition = Vector3.zero;
            boards[0].transform.rotation = startBoardChildren[1].transform.rotation;
        }
    }
}