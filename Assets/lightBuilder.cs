using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class lightBuilder : MonoBehaviour
{
    public GameObject mainHook;
	
	const int mapWidth = 13, mapHeight = 13;
	const int numberOfPoints = mapWidth * mapHeight;
	const int artNetMaxPacketSize=530;
	
	int xCnt=0, yCnt=0, dmxCnt=0;
	float pixelOffset = 0.3f;
	const float startX = -2.5f, startY = 2f;
	
	Vector3 pointPos = new Vector3(startX, startY, 0f);
	Vector3 pointScale = new Vector3(0.2f, 0.2f, 0.1f);

	GameObject[, ] virtualPixels = new GameObject[mapHeight, mapWidth];
	Color cObj;
	
	//UDP RX Stuff
	Thread receiveThread;
	int artNetPort=6454;
    UdpClient artNetClient;
	IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
	artNetPacket artNetData = new artNetPacket();
	int frameRate = 25;
	
	// Start is called before the first frame update
    void Start()
    {
		//Set up frame rate
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = frameRate;
		
		//create all the light points
		cObj.r = 1;
		cObj.g = 1;
		cObj.b = 1;
		for(yCnt=0; yCnt<mapHeight; yCnt++)
		{
			pointPos.y = startY - (pixelOffset*yCnt);
			for(xCnt=0; xCnt<mapWidth; xCnt++)
			{
				pointPos.x = startX + (pixelOffset*xCnt);
				virtualPixels[yCnt, xCnt] = GameObject.CreatePrimitive(PrimitiveType.Cube);
				virtualPixels[yCnt, xCnt].name = "X"+xCnt+"Y"+yCnt;
				virtualPixels[yCnt, xCnt].GetComponent<Renderer>().material.SetColor("_Color", cObj);
				virtualPixels[yCnt, xCnt].transform.position = pointPos;
				virtualPixels[yCnt, xCnt].transform.localScale = pointScale;
			}
		}
		
		//init OSC receive network stuff
		artNetClient = new UdpClient(artNetPort);
		receiveThread = new Thread( new ThreadStart(pollArtNet) );
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
		if(artNetData.hasChanged==1)
		{
			dumpToCubes();
			artNetData.hasChanged=0;
			//Debug.Log(artNetData.data[13]);
		}
    }
	
	
    void OnPostRender()
    {
        
    }
	
	void pollArtNet()
	{	
		while(true)
		{
			try
			{
				byte[] data = artNetClient.Receive(ref RemoteIpEndPoint); 			
				if(data.Length==artNetMaxPacketSize)
				{
					//Debug.Log(data[31]);
					artNetData.parseArtNetPacket(data);
				}
			}
			catch(Exception err)
			{
				Debug.Log(err.ToString());
			}
		}
	}
	
	void dumpToCubes()
	{
		dmxCnt = 0;
		for(yCnt=0; yCnt<mapHeight; yCnt++)
		{
			for(xCnt=0; xCnt<mapWidth; xCnt++)
			{
				cObj.r = (float)((float)artNetData.data[dmxCnt]/256);
				cObj.g = (float)((float)artNetData.data[dmxCnt+1]/256);
				cObj.b = (float)((float)artNetData.data[dmxCnt+2]/256);
				virtualPixels[yCnt, xCnt].GetComponent<Renderer>().material.SetColor("_Color", cObj);
				dmxCnt+=3;
			}
		}
	}

}

public class artNetPacket
{
	public byte[] header;       			//0-6
    public byte[] opcode;             		//8-9
    public byte[] protocolVersion;    		//10-11
    public byte sequence;              		//12
    public byte physical;              		//13
    public byte[] universe;           		//14-15
    public byte[] dataLength;         		//16-17
    public byte[] data;                 	//18-530
    public byte hasChanged;
    
    public int pCnt;
    public int pIndex;
	
	public artNetPacket()
	{
		header = new byte[7];
		opcode = new byte[2];
		protocolVersion = new byte[2];
		sequence = 0;
		physical = 0;
		universe = new byte[2];
		dataLength = new byte[2];
		data = new byte[512];
		hasChanged = 0;
		pCnt = 0;
		pIndex = 0;
	}
	
	public void parseArtNetPacket(byte[] packetBuffer)
	{
	  //header
	  pIndex=0;
	  for(pCnt=0; pCnt<7; pCnt++)
	  {
		header[pCnt] = packetBuffer[pIndex];
		pIndex++;
	  }
	  //opcode
	  pIndex++;
	  for(pCnt=0; pCnt<2; pCnt++)
	  {
		opcode[pCnt] = packetBuffer[pIndex];
		pIndex++;
	  }
	  //Protocol Version
	  for(pCnt=0; pCnt<2; pCnt++)
	  {
		protocolVersion[pCnt] = packetBuffer[pIndex];
		pIndex++;
	  }
	  //sequence
	  sequence = packetBuffer[pIndex];
	  pIndex++;
	  //physical
	  physical = packetBuffer[pIndex];
	  pIndex++;
	  //universe
	  for(pCnt=0; pCnt<2; pCnt++)
	  {
		universe[pCnt] = packetBuffer[pIndex];
		pIndex++;
	  }
	  //datalengsth
	  for(pCnt=0; pCnt<2; pCnt++)
	  {
		dataLength[pCnt] = packetBuffer[pIndex];
		pIndex++;
	  }
	  //data
	  for(pCnt=0; pCnt<512; pCnt++)
	  {
		if(data[pCnt]!=packetBuffer[pIndex] && hasChanged==0)
		{
		  hasChanged=1;
		}
		data[pCnt] = packetBuffer[pIndex];
		pIndex++;
	  }
	  //check for blank
	  pIndex=0;
	  for(pCnt=0; pCnt<512; pCnt++)
	  {
		pIndex+=data[pCnt];
	  }
	  if(pIndex==0 && hasChanged==0)
	  {
		hasChanged=1;
	  }
	}
}
