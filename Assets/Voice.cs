//Attach to player object
using UnityEngine;

using  Mirror;
 
public class Voice : NetworkBehaviour
{
 
    int lastSample;
    AudioClip audioClip;
    int FREQUENCY = 22100; //Default 44100
    int length = 50;
    private GameObject microphoneIcon;
    void Start()
    {
        Debug.Log(Microphone.devices.Length);
        Debug.Log(Microphone.devices);
        Debug.Log(Microphone.devices[0]);
        if (isLocalPlayer)
        {
            audioClip = Microphone.Start(Microphone.devices[0], true, length, FREQUENCY);
            while (Microphone.GetPosition(null) < 0) { }
            var audioSource = gameObject.GetComponent<AudioSource>();

            // var mic = Mic.Instance;
            // mic.StartRecording(16000, 100);
            
        }
    }
 
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKey(KeyCode.V))
            {
                int pos = Microphone.GetPosition(null);
                int diff = pos - lastSample;
                if (diff > 0)
                {
                    float[] samples = new float[diff * audioClip.channels];
                    audioClip.GetData(samples, lastSample);
                    byte[] ba = ToByteArray(samples);
                 
                    Cmd_SendRPC(ba, audioClip.channels);
                }
                lastSample = pos;
            }
 
        }
    }
 
    [ClientRpc]
    public void Rpc_Send(byte[] ba, int chan)
    {
        ReciveData(ba, chan);
        Debug.Log("Recieved Data " + ba.Length);
    }
 
    [Command]
    public void Cmd_SendRPC(byte[] ba, int chan)
    {
        ReciveData(ba, chan);
        Rpc_Send(ba, chan);
    }
 
    void ReciveData(byte[] ba, int chan)
    {
        float[] data = ToFloatArray(ba);
        GetComponent<AudioSource>().clip = AudioClip.Create("test", data.Length, chan, FREQUENCY, true, false);
        GetComponent<AudioSource>().clip.SetData(data, 0);
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.mute = false;
        audioSource.bypassEffects = true;
        audioSource.bypassListenerEffects = true;
        //audioSource.Play();
        if (!GetComponent<AudioSource>().isPlaying) GetComponent<AudioSource>().Play();
    }
 
    public byte[] ToByteArray(float[] floatArray)
    {
        int len = floatArray.Length * 4;
        byte[] byteArray = new byte[len];
        int pos = 0;
        foreach (float f in floatArray)
        {
            byte[] data = System.BitConverter.GetBytes(f);
            System.Array.Copy(data, 0, byteArray, pos, 4);
            pos += 4;
        }
        return byteArray;
    }
 
    public float[] ToFloatArray(byte[] byteArray)
    {
        int len = byteArray.Length / 4;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 4)
        {
            floatArray[i / 4] = System.BitConverter.ToSingle(byteArray, i);
        }
        return floatArray;
    }
}
