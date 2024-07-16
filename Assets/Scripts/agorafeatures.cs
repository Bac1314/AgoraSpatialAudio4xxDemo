using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using Agora.Util;
using UnityEngine.Networking;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.UI;


public class agorafeatures : MonoBehaviour
{

    // Fill in your app ID.
    private string _appID = "d5af82b4b85e4838b2caec2a84e71cf3";
    // Fill in your channel name.
    private string _channelName = "channel_bac";
    // Fill in the temporary token you obtained from Agora Console.
    private string _token = "";
    // A variable to save the remote user uid.
    public uint remoteUid;
    internal IRtcEngine mRtcEngine;

    // Spatial Audio Variables
    public ILocalSpatialAudioEngine localSpatialEngine;
    private bool spatialEnabled = false;
    public GameObject localUserRobot;
    public GameObject remoteUserRobot;
    float elapsed = 0f;
    float timeToUpdateSpatial = 0.1f; // Update location every 0.1 second


    // Start is called before the first frame update
    void Start()
    {
        SetupVideoSDKEngine();
        configureSpatialAudioEngine();
        JoinChannel();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        // If spatial is enabled update the local and remote position every timeToUpdateSpatial second
        if (spatialEnabled)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeToUpdateSpatial)
            {
                elapsed = elapsed % timeToUpdateSpatial;
                updateSpatialAudioPosition();
            }
        }


    }

    private void OnDestroy()
    {
        if (mRtcEngine == null) return;
        mRtcEngine.LeaveChannel();
        mRtcEngine.Dispose();
    }

    private void SetupVideoSDKEngine()
    {
        mRtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext(_appID, 0, CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        
        var ret = mRtcEngine.Initialize(context);

        Debug.Log("SetupVideoSDKEngine initialize agora " + ret);

        mRtcEngine.InitEventHandler(handler);
    }

    private void JoinChannel()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.JoinChannel(_token, _channelName, "", 0);
        }

    }

    private void configureSpatialAudioEngine()
    {
        spatialEnabled = true;

        mRtcEngine.EnableAudio();
        mRtcEngine.EnableSpatialAudio(true);

        localSpatialEngine = mRtcEngine.GetLocalSpatialAudioEngine();
        var ret = localSpatialEngine.Initialize();

        Debug.Log("Bac's configureSpatialAudioEngine localSpatialEngine initliaze result " + ret);

        localSpatialEngine.MuteLocalAudioStream(true);
        //localSpatialEngine.MuteAllRemoteAudioStreams(true);
        localSpatialEngine.SetAudioRecvRange(50);
        localSpatialEngine.SetDistanceUnit(1);


    }

    public void updateSpatialAudioPosition()
    {
        // Setup self position
        float[] pos = new float[] { localUserRobot.transform.position.x, localUserRobot.transform.position.y, localUserRobot.transform.position.z };
        float[] forward = new float[] { localUserRobot.transform.forward.x, localUserRobot.transform.forward.y, localUserRobot.transform.forward.z };
        float[] right = new float[] { localUserRobot.transform.right.x, localUserRobot.transform.right.y, localUserRobot.transform.right.z };
        float[] up = new float[] { localUserRobot.transform.up.x, localUserRobot.transform.up.y, localUserRobot.transform.up.z };
        localSpatialEngine.UpdateSelfPosition(pos, forward, right, up);

        // Setup remote position
        float[] pos1 = new float[] { remoteUserRobot.transform.position.x, remoteUserRobot.transform.position.y, remoteUserRobot.transform.position.z };
        float[] forward1 = new float[] { remoteUserRobot.transform.forward.x, remoteUserRobot.transform.forward.y, remoteUserRobot.transform.forward.z };
        RemoteVoicePositionInfo remotePosInfo = new RemoteVoicePositionInfo(pos1, forward1);
        localSpatialEngine.UpdateRemotePosition(remoteUid, remotePosInfo);
    }


}


internal class UserEventHandler : IRtcEngineEventHandler
{
    private readonly agorafeatures _agorafeatures;

    internal UserEventHandler(agorafeatures agorafeatures)
    {
        _agorafeatures = agorafeatures;
    }

    // This callback is triggered when the local user joins the channel.
    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
    }

    public override void OnUserJoined(RtcConnection connection, uint remoteUid, int elapsed)
    {
        _agorafeatures.remoteUid = remoteUid;
        Debug.Log("Bac's OnUserjoined uid " + remoteUid);
        _agorafeatures.localSpatialEngine.MuteAllRemoteAudioStreams(false);
        _agorafeatures.updateSpatialAudioPosition();
    }


}