using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace VrepWrapper
{

    public static class Vrep
    {
        
        private const string dllpath = "libraries/remoteApi.dll";

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandReturnCodes simxTransferFile(int clientId, string filePathAndName,
            string fileName_serverSide, int timeout, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandReturnCodes simxEraseFile(int clientId,
            string fileName_serverSide, RegularOperationMode operationMode);


        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void simxCallScriptFunction(int clientID, string scriptDescription, ScriptHandleOrType scriptHandleOrType, string functionName, int inIntCnt, int[] inIntptr, int inFloatCnt, float[] inFloat, int inStringCnt, string inString, int inBufferSize, string inBuffer, ref int outIntCnt, ref IntPtr outInt, ref int outFloatCnt, ref IntPtr outFloat, ref int outStringCnt, ref IntPtr outString, ref int outBufferSize, ref IntPtr outBuffer, RegularOperationMode operationMode);


        public static void SimCallScriptFunction(int clientID, string scriptDescription, ScriptHandleOrType scriptHandleOrType, string functionName, int inIntCnt, int[] inIntptr, int inFloatCnt, float[] inFloat, int inStringCnt, string inString, int inBufferSize, string inBuffer, ref int[] outInt, ref float[] outFloat, ref string[] outString, ref char[] outBuffer)
        {
            IntPtr outIntptr = IntPtr.Zero;
            int outIntCnt = 0;
            IntPtr outFloatptr = IntPtr.Zero;
            int outFloatCnt = 0;
            IntPtr outStringptr = IntPtr.Zero;
            int outStringCnt = 0;
            IntPtr outBufferptr = IntPtr.Zero;
            int outBufferCnt = 0;
            simxCallScriptFunction(clientID, scriptDescription, scriptHandleOrType, functionName, inIntCnt, inIntptr, inFloatCnt, inFloat, inStringCnt, inString, inBufferSize, inBuffer, ref outIntCnt, ref outIntptr, ref outFloatCnt, ref outFloatptr, ref outStringCnt, ref outStringptr, ref outBufferCnt, ref outBufferptr, RegularOperationMode.SimxOpmodeBlocking);

            outInt = new int[outIntCnt];
            outFloat = new float[outFloatCnt];
            outString = new string[outStringCnt];
            outBuffer = new char[outBufferCnt];

            if (outIntptr != IntPtr.Zero)
            {
                Marshal.Copy(outIntptr, outInt, 0, outIntCnt);

            }
            if (outFloatptr != IntPtr.Zero)
            {
                Marshal.Copy(outFloatptr, outFloat, 0, outFloatCnt);
            }
            if (outStringptr != IntPtr.Zero)
            {
                for (int i = 0; i < outStringCnt; i++)
                {
                    int offset = 0;
                    string currentString = null;
                    while (true)
                    {
                        byte myByte = Marshal.ReadByte(outStringptr, offset);
                        offset++;
                        var c = Encoding.ASCII.GetString(new byte[] { myByte });
                        if (c != "\0")
                        {
                            currentString += c;
                        }
                        else
                        {
                            outString[i] = currentString;
                            break;
                        }

                    }

                }

            }

            byte[] dataBytes = new byte[outBufferCnt];
            if (outBufferptr != IntPtr.Zero)
            {


                int offset = 0;
                while (true)
                {
                    var myByte = Marshal.ReadByte(outBufferptr, offset);
                    if (myByte != 0)
                    {
                        dataBytes[offset] = myByte;
                    }
                    else
                    {
                        break;
                    }

                    offset++;
                }


            }
            outBuffer = Encoding.ASCII.GetChars(dataBytes);

        }



        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxGetModelProperty(int clientId, int objectHandle, ref int property, RegularOperationMode operationMode);

        public static void SimGetObjectPoses(int clientId, SceneObjectTypes objectTypes, ref int[] objectHandles, ref float[][] objectPoses)
        {
            int objectCount = -1;
            IntPtr objectHandlesPtr = IntPtr.Zero;

            int intDataCount = -1;
            int positionDataCount = -1;
            int orientationDataCount = -1;
            int stringDataCount = -1;

            IntPtr intDataPtr = IntPtr.Zero;
            IntPtr positionDataPtr = IntPtr.Zero;
            IntPtr orientationDataPtr = IntPtr.Zero;
            IntPtr stringDataPtr = IntPtr.Zero;

            simxGetObjectGroupData(clientId, objectTypes, 3, ref objectCount, ref objectHandlesPtr,
                ref intDataCount, ref intDataPtr, ref positionDataCount, ref positionDataPtr, ref stringDataCount,
                ref stringDataPtr, Vrep.RegularOperationMode.SimxOpmodeBlocking);

            if (objectCount != 0)
            {
                objectHandles = new int[objectCount];
                Marshal.Copy(objectHandlesPtr, objectHandles, 0, objectCount);
            }

            if (positionDataCount != 0)
            {
                objectPoses = new float[objectCount][];
                float[] currentFloatData = new float[positionDataCount];
                Marshal.Copy(positionDataPtr, currentFloatData, 0, positionDataCount);
                for (int i = 0; i < objectCount; i++)
                {
                    objectPoses[i] = new float[6] { (currentFloatData[i * 3]), currentFloatData[(i * 3) + 1], currentFloatData[(i * 3) + 2], 0.0f, 0.0f, 0.0f };
                }
            }

            simxGetObjectGroupData(clientId, objectTypes, 5, ref objectCount, ref objectHandlesPtr,
                ref intDataCount, ref intDataPtr, ref orientationDataCount, ref orientationDataPtr, ref stringDataCount,
                ref stringDataPtr, Vrep.RegularOperationMode.SimxOpmodeBlocking);

            if (objectCount != 0)
            {
                objectHandles = new int[objectCount];
                Marshal.Copy(objectHandlesPtr, objectHandles, 0, objectCount);
            }

            if (orientationDataCount != 0)
            {
                float[] currentOrientationData = new float[orientationDataCount];
                Marshal.Copy(orientationDataPtr, currentOrientationData, 0, orientationDataCount);
                for (int i = 0; i < objectCount; i++)
                {
                    objectPoses[i][3] = currentOrientationData[i * 3] * (180.0f / Convert.ToSingle(Math.PI));
                    objectPoses[i][4] = currentOrientationData[(i * 3) + 1] * (180.0f / Convert.ToSingle(Math.PI));
                    objectPoses[i][5] = currentOrientationData[(i * 3) + 2] * (180.0f / Convert.ToSingle(Math.PI));
                }
            }

        }

        public static void SimGetObjectNames(int clientId, SceneObjectTypes objectTypes, ref int[] objectHandles, ref string[] objectNames, RegularOperationMode operationMode)
        {
            int objectCount = -1;
            IntPtr objectHandlesPtr = IntPtr.Zero;

            int intDataCount = -1;
            int floatDataCount = -1;
            int stringDataCount = -1;

            IntPtr intDataPtr = IntPtr.Zero;
            IntPtr floatDataPtr = IntPtr.Zero;
            IntPtr stringDataPtr = IntPtr.Zero;

            simxGetObjectGroupData(clientId, objectTypes, 0, ref objectCount, ref objectHandlesPtr,
                ref intDataCount, ref intDataPtr, ref floatDataCount, ref floatDataPtr, ref stringDataCount,
                ref stringDataPtr, Vrep.RegularOperationMode.SimxOpmodeBlocking);


            if (objectCount != 0)
            {
                objectHandles = new int[objectCount];
                Marshal.Copy(objectHandlesPtr, objectHandles, 0, objectCount);
            }

            if (stringDataCount != 0)
            {
                //   byte[] receivedWords = new byte[100000];
                int offset = 0;
                string[] seperator = new string[] { "\0" };
                string currentWord = string.Empty;
                objectNames = new string[stringDataCount];
                int currentWordOffset = 0;

                while (currentWordOffset < stringDataCount)
                {

                    byte currentByte = Marshal.ReadByte(stringDataPtr, offset);
                    offset++;

                    string c = System.Text.Encoding.ASCII.GetString(new[] { currentByte });

                    if (c.Equals(seperator[0]))
                    {
                        objectNames[currentWordOffset] = currentWord;
                        currentWord = string.Empty;
                        currentWordOffset++;
                    }
                    else
                    {
                        currentWord += c;
                    }
                }

            }

        }

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxGetObjectGroupData(int clientId, SceneObjectTypes objectTypes, int dataType, ref int objectCount, ref IntPtr objectHandles, ref int intDataCount, ref IntPtr intData, ref int floatDataCount, ref IntPtr floatData, ref int stringDataCount, ref IntPtr stringData, RegularOperationMode operationMode);

        public static int[] SimGetObjects(int clientId, int objectTypes, RegularOperationMode operationMode)
        {
            int objectCount = -1;
            int[] objectHandles = null;
            IntPtr objectHandlesPtr = IntPtr.Zero;
            simxGetObjects(clientId, objectTypes, ref objectCount, ref objectHandlesPtr, operationMode);

            if (objectCount != 0)
            {
                objectHandles = new int[objectCount];
                Marshal.Copy(objectHandlesPtr, objectHandles, 0, objectCount);
            }
            return objectHandles;
        }

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxGetObjects(int clientID, int objectTypes, ref int objectCount, ref IntPtr objectHandles, RegularOperationMode operation);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandReturnCodes simxGetObjectOrientation(int clientID, int objectHandle, int relativeToObjectHandle, ref float[] orientation, RegularOperationMode operationMode);

        /* public static float[] simGetObjectPostion(int clientID, int objectHandle, int relativeToObjectHandle)
          {
              float[] position = null ;
              Vrep.simxGetObjectPosition(clientID, objectHandle, relativeToObjectHandle, ref position, RegularOperationMode.SimxOpmodeBlocking);
              return position;
          }*/

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandReturnCodes simxGetObjectPosition(int clientID, int objectHandle, int relativeToObjectHandle, ref float position, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxCreateDummy(int clientID, float size, char[] color, out int dummyHandle, RegularOperationMode operationMode);

        public static void simxSetObjectPose(int clientID, int handle, int relativeToObjectHandle, VRepCosy pose, RegularOperationMode operationMode)
        {
            simxSetObjectPosition(clientID, handle, relativeToObjectHandle, toFloat(pose.Position), operationMode);
            simxSetObjectOrientation(clientID, handle, relativeToObjectHandle, toFloat(pose.Orientation), operationMode);
        }

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxRemoveObject(int clientID, int objectHandle, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxSetObjectOrientation(int clientID, int handle, int relativeToObjectHandle, float[] orientation, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxSetObjectPosition(int clientID, int handle, int relativeToObjectHandle, float[] position, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxStartSimulation(int clientID, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxPauseSimulation(int clientID, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxStopSimulation(int clientID, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int simxLoadModel(int clientID, string modelPathAndName, bool options, out int handle, RegularOperationMode operationMode);
        /// <summary>
        ///Loads a previously saved scene. Should only be called when simulation is not running and is only executed by continuous remote API server services
        /// </summary>
        /// <param name="clientId"> the client ID. refer to simxStart.</param>
        /// <param name="scenePathAndName">path to the scene including ".ttt"</param>
        /// /// <param name="options">bit coded; if set, .ttt on client side(transfer scene!) otherwise on server side".ttt"</param>
        /// <param name="operationMode"> a remote API function operation mode. Recommended operation mode for this function is simx_opmode_oneshot</param>
        /// <returns></returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int simxLoadScene(int clientID, string scenePathAndName, int options, RegularOperationMode operationMode);


        /// <summary>
        /// Adds a message to the status bar.
        /// </summary>
        /// <param name="clientId"> the client ID. refer to simxStart.</param>
        /// <param name="message">the message to display</param>
        /// <param name="operationMode"> a remote API function operation mode. Recommended operation mode for this function is simx_opmode_oneshot</param>
        /// <returns></returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void simxAddStatusbarMessage(int clientId, string message, RegularOperationMode operationMode);

        /// <summary>
        /// (Managed) Adds a message to the status bar.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="message">the message to display</param>
        public static void SimAddStatusbarMessage(int clientId, string message)
        {
            //if (message == null) throw new ArgumentNullException(nameof(message));
            simxAddStatusbarMessage(clientId, message, RegularOperationMode.SimxOpmodeOneshot);
        }

        /// <summary>
        /// Returns the ID of the current connection. Use this function to track the connection state to the server. See also simxStart. This is a remote API helper function.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <returns></returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SimxGetConnectionId(int clientId);

        /// <summary>
        /// Starts a communication thread with the server (i.e. V-REP). A same client may start several communication threads (but only one communication thread for a given IP and port). This should be the very first remote API function called on the client side. Make sure to start an appropriate remote API server service on the server side, that will wait for a connection. See also simxFinish. This is a remote API helper function.
        /// </summary>
        /// <param name="connectionAddress"> the IP address where the server is located (i.e. V-REP)</param>
        /// <param name="connectionPort">the port number where to connect</param>
        /// <param name="waitUntilConnected"> if different from zero, then the function blocks until connected (or timed out).</param>
        /// <param name="doNotReconnectOnceDisconnected">if different from zero, then the communication thread will not attempt a second connection if a connection was lost.</param>
        /// <param name="timeOutInMs">
        /// if positive: the connection time-out in milliseconds for the first connection attempt. In that case, the time-out for blocking function calls is 5000 milliseconds.
        /// if negative: its positive value is the time-out for blocking function calls.In that case, the connection time-out for the first connection attempt is 5000 milliseconds.
        /// </param>
        /// <param name="commThreadCycleInMs"> indicates how often data packets are sent back and forth. Reducing this number improves responsiveness, and a default value of 5 is recommended.</param>
        /// <returns>the client ID, or -1 if the connection to the server was not possible (i.e. a timeout was reached). A call to simxStart should always be followed at the end with a call to <c>simxFinish</c> if <c>simxStart</c> didn't return -1</returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int simxStart(string connectionAddress, int connectionPort, bool waitUntilConnected, bool doNotReconnectOnceDisconnected, int timeOutInMs, int commThreadCycleInMs);

        /// <summary>
        /// Ends the communication thread. This should be the very last remote API function called on the client side. simxFinish should only be called after a successful call to simxStart. This is a remote API helper function.
        /// </summary>
        /// <param name="clientId">the client ID. refer to <c>Vrep.simxStart</c>. Can be -1 to end all running communication threads.</param>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxFinish(int clientId);

        /// <summary>
        /// Retrieves an object handle based on its name. If the client application is launched from a child script, then you could also let the child script figure out what handle correspond to what objects, and send the handles as additional arguments to the client application during its launch. See also simxGetObjectGroupData.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="objectName">name of the object. If possible, don't rely on the automatic name adjustment mechanism, and always specify the full object name, including the #: if the object is "myJoint", specify "myJoint#", if the object is "myJoint#0", specify "myJoint#0", etc. </param>
        /// <param name="handle">pointer to a value that will receive the handle</param>
        /// <param name="operationMode">a remote API function operation mode. Recommended operation mode for this function is simx_opmode_blocking</param>
        /// <returns></returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern CommandReturnCodes simxGetObjectHandle(int clientId, string objectName, out int handle, RegularOperationMode operationMode);

        /// <summary>
        /// (Managed) Retrieves an object handle based on its name. If the client application is launched from a child script, then you could also let the child script figure out what handle correspond to what objects, and send the handles as additional arguments to the client application during its launch. See also simxGetObjectGroupData.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="objectName">name of the object. If possible, don't rely on the automatic name adjustment mechanism, and always specify the full object name, including the #: if the object is "myJoint", specify "myJoint#", if the object is "myJoint#0", specify "myJoint#0", etc. </param>
        /// <returns>a value that will receive the handle</returns>
        public static int SimGetObjectHandle(int clientId, string objectName)
        {
            int handle;
            simxGetObjectHandle(clientId, objectName, out handle, RegularOperationMode.SimxOpmodeOneshotWait);
            return handle;
        }

        /// <summary>
        /// Sets the target position of a joint if the joint is in torque/force mode (also make sure that the joint's motor and position control are enabled). See also simxSetJointPosition.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <param name="targetPosition">target position of the joint (angular or linear value depending on the joint type)</param>
        /// <param name="operationMode"> a remote API function operation mode. Recommended operation modes for this function are simx_opmode_oneshot or simx_opmode_streaming</param>
        /// <returns></returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern CommandReturnCodes simxSetJointTargetPosition(int clientId, int jointHandle, float targetPosition, RegularOperationMode operationMode);

        /// <summary>
        /// (Managed) Sets the target position of a joint if the joint is in torque/force mode (also make sure that the joint's motor and position control are enabled). See also simxSetJointPosition.
        /// </summary>
        /// <param name="clientId"> the client ID. refer to simxStart.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <param name="targetPosition"> target position of the joint (angular (radian) or linear value depending on the joint type)</param>
        /// <returns></returns>
        public static CommandReturnCodes SimSetJointTargetPosition(int clientId, int jointHandle, float targetPosition)
        {
            //Call function with non-blocking fashion
            CommandReturnCodes x = simxSetJointTargetPosition(clientId, jointHandle, targetPosition, RegularOperationMode.SimxOpmodeOneshot);
            return x;
        }

        /// <summary>
        /// Retrieves the intrinsic position of a joint. This function cannot be used with spherical joints (use simxGetJointMatrix instead). See also simxSetJointPosition and simxGetObjectGroupData.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <param name="targetPosition"> intrinsic position of the joint. This is a one-dimensional value: if the joint is revolute, the rotation angle is returned, if the joint is prismatic, the translation amount is returned, etc.</param>
        /// <param name="operationMode">a remote API function operation mode. Recommended operation modes for this function are simx_opmode_streaming (the first call) and simx_opmode_buffer (the following calls)</param>
        /// <returns>a remote API function return code</returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern CommandReturnCodes simxGetJointPosition(int clientId, int jointHandle, ref float targetPosition, RegularOperationMode operationMode);

        /// <summary>
        /// (Managed) This method is called to start streaming of joint position (called once)
        /// </summary>
        /// <param name="clientId">the client ID. refer to <c>simxStart</c>.</param>
        /// <param name="jointHandle">handle of the joint</param>
        public static void SimGetJointPositionInit(int clientId, int jointHandle)
        {
            float pos = 0;
            simxGetJointPosition(clientId, jointHandle, ref pos, RegularOperationMode.SimxOpmodeStreaming);
        }

        /// <summary>
        /// (Managed) This method is called to end streaming of joint position (called once)
        /// </summary>
        /// <param name="clientId">the client ID. refer to <c>simxStart</c>.</param>
        /// <param name="jointHandle">handle of the joint</param>
        public static void SimGetJointPositionEnd(int clientId, int jointHandle)
        {
            float pos = 0;
            simxGetJointPosition(clientId, jointHandle, ref pos, RegularOperationMode.SimxOpmodeDiscontinue);
        }

        /// <summary>
        ///  (Managed) Retrieves the intrinsic position of a joint in Radian. 
        /// </summary>
        /// <param name="clientId">the client ID. refer to <c>simxStart</c>.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <returns>joint position in radian</returns>
        public static float SimGetJointPositionRadian(int clientId, int jointHandle)
        {
            float pos = 0;
            simxGetJointPosition(clientId, jointHandle, ref pos, RegularOperationMode.SimxOpmodeBuffer);
            return pos;
        }

        /// <summary>
        ///  (Managed) Retrieves the intrinsic position of a joint.
        /// </summary>
        /// <param name="clientId">the client ID. refer to <c>simxStart</c>.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <returns>joint position in degrees</returns>
        public static float SimGetJointPositionDegrees(int clientId, int jointHandle)
        {
            float pos = 0;
            simxGetJointPosition(clientId, jointHandle, ref pos, RegularOperationMode.SimxOpmodeBuffer);
            return (float)Math.Round(pos * (180.0 / Math.PI), 2);
        }

        /// <summary>
        /// Sets the intrinsic target velocity of a non-spherical joint. This command makes only sense when the joint mode is in torque/force mode: the dynamics functionality and the joint motor have to be enabled (position control should however be disabled)
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <param name="velocity">target velocity of the joint (linear or angular velocity depending on the joint-type)</param>
        /// <param name="operationMode"> remote API function operation mode. Recommended operation modes for this function are simx_opmode_oneshot or simx_opmode_streaming</param>
        /// <returns>a remote API function return code</returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern CommandReturnCodes simxSetJointTargetVelocity(int clientId, int jointHandle, float velocity, RegularOperationMode operationMode);

        /// <summary>
        /// Sets the intrinsic target velocity of a non-spherical joint. This command makes only sense when the joint mode is in torque/force mode: the dynamics functionality and the joint motor have to be enabled (position control should however be disabled)
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="jointHandle">handle of the joint</param>
        /// <param name="velocity">target velocity of the joint (linear or angular velocity depending on the joint-type)</param>
        /// <returns></returns>
        public static void SimSetJointTargetVelocity(int clientId, int jointHandle, float velocity)
        {
            simxSetJointTargetVelocity(clientId, jointHandle, velocity, RegularOperationMode.SimxOpmodeOneshot);
        }

        /// <summary>
        /// Gets the value of a float signal. Signals are cleared at simulation start. See also simxSetFloatSignal, simxClearFloatSignal, simxGetIntegerSignal and simxGetStringSignal.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <param name="signalValue">pointer to a location receiving the value of the signal</param>
        /// <param name="operationMode">a remote API function operation mode. Recommended operation modes for this function are simx_opmode_streaming (the first call) and simx_opmode_buffer (the following calls)</param>
        /// <returns> remote API function return code</returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern CommandReturnCodes simxGetFloatSignal(int clientId, string signalName, ref float signalValue, RegularOperationMode operationMode);

        /// <summary>
        /// (Managed) Start streaming of float signal value (called once)
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <returns>the value of the signal</returns>
        public static float SimGetFloatSignalInit(int clientId, string signalName)
        {
            float val = 0;
            simxGetFloatSignal(clientId, signalName, ref val, RegularOperationMode.SimxOpmodeStreaming);
            return val;
        }

        /// <summary>
        /// (Managed) Start streaming of float signal value (called once)
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <returns>the value of the signal</returns>
        public static CommandReturnCodes SimGetFloatSignalEnd(int clientId, string signalName)
        {
            float val = 0;
            var e = simxGetFloatSignal(clientId, signalName, ref val, RegularOperationMode.SimxOpmodeDiscontinue);
            return e;
        }

        /// <summary>
        /// (Managed) Streaming of float signal value. This should be called after SimGetFloatSignalInit
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <returns>the value of the signal</returns>
        public static CommandReturnCodes SimGetFloatSignal(int clientId, string signalName)
        {
            float val = 0;
            var e = simxGetFloatSignal(clientId, signalName, ref val, RegularOperationMode.SimxOpmodeStreaming);
            return e;
        }

        /// <summary>
        /// Gets the value of an integer signal. Signals are cleared at simulation start. See also simxSetIntegerSignal, simxClearIntegerSignal, simxGetFloatSignal and simxGetStringSignal.
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <param name="signalValue">pointer to a location receiving the value of the signal</param>
        /// <param name="operationMode"> a remote API function operation mode. Recommended operation modes for this function are simx_opmode_streaming (the first call) and simx_opmode_buffer (the following calls)</param>
        /// <returns>a remote API function return code</returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        private static extern CommandReturnCodes simxGetIntegerSignal(int clientId, string signalName, ref int signalValue, RegularOperationMode operationMode);

        /// <summary>
        /// (Managed) Start streaming of integer signal value (called once)
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <returns>Error code</returns>
        public static CommandReturnCodes SimGetIntegerSignalInit(int clientId, string signalName)
        {
            int val = 0;
            var e = simxGetIntegerSignal(clientId, signalName, ref val, RegularOperationMode.SimxOpmodeStreaming);
            return e;
        }

        /// <summary>
        /// (Managed) Start streaming of integer signal value (called once)
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <returns>Error code</returns>
        public static CommandReturnCodes SimGetIntegerSignalEnd(int clientId, string signalName)
        {
            int val = 0;
            var e = simxGetIntegerSignal(clientId, signalName, ref val, RegularOperationMode.SimxOpmodeDiscontinue);
            return e;
        }

        /// <summary>
        /// (Managed) Streaming of integer signal value. This should be called after SimGetIntegerSignalInit
        /// </summary>
        /// <param name="clientId">the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <returns>the value of the signal</returns>
        public static int SimGetIntegerSignal(int clientId, string signalName)
        {
            int val = 0;
            simxGetIntegerSignal(clientId, signalName, ref val, RegularOperationMode.SimxOpmodeStreaming);
            return val;
        }

        /// <summary>
        /// Gets the value of a string signal. Signals are cleared at simulation start. See also simxSetStringSignal, simxReadStringStream, simxClearStringSignal, simxGetIntegerSignal and simxGetFloatSignal.
        /// </summary>
        /// <param name="clientId"> the client ID. refer to simxStart.</param>
        /// <param name="signalName">name of the signal</param>
        /// <param name="signalValue"> pointer to a pointer receiving the value of the signal. The signal value will remain valid until next remote API call</param>
        /// <param name="signalLength">pointer to a location receiving the value of the signal length, since it may contain any data (also embedded zeros).</param>
        /// <param name="operationMode"> a remote API function operation mode. Recommended operation modes for this function are simx_opmode_streaming (the first call) and simx_opmode_buffer (the following calls)</param>
        /// <returns>a remote API function return code</returns>
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern CommandReturnCodes simxGetStringSignal(int clientId, string signalName, ref IntPtr signalValue, ref int signalLength, RegularOperationMode operationMode);

        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxSetObjectParent(int clientID, int objectHandle, int parentObject,
            byte keepInPlace, RegularOperationMode operationMode);
        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxGetObjectChild(int clientID, int parentObjectHandle, int childindex,
            ref int childObjectHandle, RegularOperationMode operationMode);


        [DllImport(dllpath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void simxSetModelProperty(int clientId, int objectHandle, ref int property, RegularOperationMode operationMode);

        public static int SimxCreateCollection(int clientID, string collectionName)
        {

            int[] inIntegers = { clientID }, outIntegers = { 0 };
            float[] inFloats = { }, outFloats = null;
            string inBuffer = null, inStrings = collectionName;
            string[] outStrings = null;
            char[] outBuffer = null;

            int outIntCnt = 1, outFloatCnt = 0;



            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxCreateCollection",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, inStrings.Length, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            Vrep.SimAddStatusbarMessage(clientID, "Created new collection: " + collectionName + " Handle: " + outIntegers[0]);
            return outIntegers[0];
        }

        public static int SimxEmptyCollection(int clientID, int collectionHandle)
        {

            int[] inIntegers = { collectionHandle }, outIntegers = { 0 };
            float[] inFloats = { }, outFloats = null;
            string inBuffer = null, inStrings = null;
            string[] outStrings = null;
            char[] outBuffer = null;
            int outIntCnt = 1, outFloatCnt = 0, outStringCnt = 0, outByteCnt = 0;

            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxEmptyCollection",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, 0, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            Vrep.SimAddStatusbarMessage(clientID, "Deleted collection: " + inIntegers[0]);
            return outIntegers[0];
        }

        public static int SimxAddObjectToCollection(int clientID, int collectionHandle, int objectHandle)
        {
            return SimxAddObjectToCollection(clientID, collectionHandle, objectHandle, specialArgumentValues.sim_handle_single,
                0);
        }
        public static int SimxRemoveObjectFromCollection(int clientID, int collectionHandle, int objectHandle)
        {
            return SimxAddObjectToCollection(clientID, collectionHandle, objectHandle, specialArgumentValues.sim_handle_single,
                1);
        }

        public static int SimxAddObjectToCollection(int clientID, int collectionHandle, int objectHandle, specialArgumentValues what,
            int options)
        {

            int[] inIntegers = { collectionHandle, objectHandle, (int)what, options }, outIntegers = { 0 };
            float[] inFloats = { }, outFloats = null;
            string inBuffer = null, inStrings = null;
            string[] outStrings = null;
            char[] outBuffer = null;
            int outIntCnt = 1, outFloatCnt = 0;

            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxAddObjectToCollection",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, 0, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            Vrep.SimAddStatusbarMessage(clientID, "Collection Member added/removed: " + inIntegers[1]);
            return outIntegers[0];
        }

        public static int SimxRemoveCollection(int clientID, int collectionHandle)
        {

            int[] inIntegers = { collectionHandle }, outIntegers = { 0 };
            float[] inFloats = { }, outFloats = null;
            string inBuffer = null, inStrings = null;
            string[] outStrings = null;
            char[] outBuffer = null;
            int outIntCnt = 1, outFloatCnt = 0;

            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxRemoveCollection",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, 0, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            Vrep.SimAddStatusbarMessage(clientID, "Collection removed " + inIntegers[0]);
            return outIntegers[0];
        }

        public static bool SimxCheckcollision(int clientID, int entity1Handle, int entity2Handle)
        {

            int[] inIntegers = { entity1Handle, entity2Handle }, outIntegers = { 0 };
            float[] inFloats = { }, outFloats = null;
            string inBuffer = null, inStrings = null;
            string[] outStrings = null;
            char[] outBuffer = null;
            int outIntCnt = 1, outFloatCnt = 0;

            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxCheckCollision",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, 0, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            Vrep.SimAddStatusbarMessage(clientID, "Collision executed " + entity1Handle + " & " + entity2Handle + Convert.ToBoolean(outIntegers[0]));
            if (outIntegers[0] == -1)
            { throw new Exception("Vrep collision error (Handles correct?)"); }
            else
            { return Convert.ToBoolean(outIntegers[0]); }

        }
        public static int SimxCreateChildCube(int clientID, int ParentHandle, float cubesize_mm)
        {

            int[] inIntegers = { ParentHandle }, outIntegers = { 0 };
            float[] inFloats = { cubesize_mm * 0.001f }, outFloats = null;
            string inBuffer = null, inStrings = null;
            string[] outStrings = null;
            char[] outBuffer = null;
            int outIntCnt = 1, outFloatCnt = 0;

            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxCreateCube",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, 0, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            //Vrep.SimAddStatusbarMessage(clientID, "created cube");
            if (outIntegers[0] == -1)
            { throw new Exception("Vrep Cube Creation Error (Handles correct?)"); }
            else
            { return outIntegers[0]; }

        }

        public static void SimxSetObjectSpecialProperty(int clientID, int ObjectHandle, SceneObjectMainProperties Property)
        {

            int[] inIntegers = { ObjectHandle, (int)Property }, outIntegers = { 0 };
            float[] inFloats = { }, outFloats = null;
            string inBuffer = null, inStrings = null;
            string[] outStrings = null;
            char[] outBuffer = null;

            int outIntCnt = 1, outFloatCnt = 0;

            Vrep.SimCallScriptFunction(clientID, "IntersectionBase", Vrep.ScriptHandleOrType.sim_scripttype_childscript, "simxSetObjectSpecialProperty",
                inIntegers.Length, inIntegers, inFloats.Length, inFloats, 0, inStrings, 0, inBuffer, ref outIntegers, ref outFloats, ref outStrings, ref outBuffer);
            //Vrep.SimAddStatusbarMessage(clientID, "created cube");
            if (outIntegers[0] == -1)
            { throw new Exception("Vrep Property setting failed (ObjectHandles correct?)"); }

        }



        // Command return codes
        public enum CommandReturnCodes
        {
            SimxReturnOk = 0,
            SimxReturnNovalueFlag = 1,
            SimxReturnTimeoutFlag = 2,
            SimxReturnIllegalOpmodeFlag = 4,
            SimxReturnRemoteErrorFlag = 8,
            SimxReturnSplitProgressFlag = 16,
            SimxReturnLocalErrorFlag = 32,
            SimxReturnInitializeErrorFlag = 64,
        }

        // Special argument values
        public enum specialArgumentValues
        {
            sim_handle_all = -2,
            sim_handle_all_except_explicit = -3,
            sim_handle_self = -4,
            sim_handle_main_script = -5,
            sim_handle_tree = -6,
            sim_handle_chain = -7,
            sim_handle_single = -8,
            sim_handle_default = -9,
            sim_handle_all_except_self = -10,
            sim_handle_parent = -11,
            sim_handle_scene = -12,
            sim_handle_app = -13
        }

        // Regular operation modes
        public enum RegularOperationMode
        {
            // Regular operation modes
            SimxOpmodeOneshot = 0,
            SimxOpmodeBlocking = 65536,
            SimxOpmodeOneshotWait = 65536,
            SimxOpmodeContinuous = 131072,
            SimxOpmodeStreaming = 131072,

            // Operation modes for heavy data
            SimxOpmodeOneshotSplit = 196608,
            SimxOpmodeContinuousSplit = 262144,
            SimxOpmodeStreamingSplit = 262144,

            // Special operation modes
            SimxOpmodeDiscontinue = 327680,
            SimxOpmodeBuffer = 393216,
            SimxOpmodeRemove = 458752,
        }

        // Scene object types
        public enum SceneObjectTypes
        {
            SimObjectShapeType = 0,
            SimObjectJointType = 1,
            SimObjectGraphType = 2,
            SimObjectCameraType = 3,
            SimObjectDummyType = 4,
            SimObjectProximitysensorType = 5,
            SimObjectReserved1 = 6,
            SimObjectReserved2 = 7,
            SimObjectPathType = 8,
            SimObjectVisionsensorType = 9,
            SimObjectVolumeType = 10,
            SimObjectMillType = 11,
            SimObjectForcesensorType = 12,
            SimObjectLightType = 13,
            SimObjectMirrorType = 14,
        }

        // General object types
        public enum GeneralObjectTypes
        {
            SimAppobjObjectType = 109,
            SimAppobjCollisionType = 110,
            SimAppobjDistanceType = 111,
            SimAppobjSimulationType = 112,
            SimAppobjIkType = 113,
            SimAppobjConstraintsolverType = 114,
            SimAppobjCollectionType = 115,
            SimAppobjUiType = 116,
            SimAppobjScriptType = 117,
            SimAppobjPathplanningType = 118,
            SimAppobjReservedType = 119,
            SimAppobjTextureType = 120,
        }

        // Inverse Kinematics calculation methods
        public enum IkCalculationMethods
        {
            SimIkPseudoInverseMethod = 0,
            SimIkDampedLeastSquaresMethod = 1,
            SimIkJacobianTransposeMethod = 2,
        }

        // Inverse Kinematics constraints
        public enum IkConstraints
        {
            SimIkXConstraint = 1,
            SimIkYConstraint = 2,
            SimIkZConstraint = 4,
            SimIkAlphaBetaConstraint = 8,
            SimIkGammaConstraint = 16,
            SimIkAvoidanceConstraint = 64,
        }

        //  Inverse Kinematics calculation results
        public enum IkCalculationResults
        {
            SimIkresultNotPerformed = 0,
            SimIkresultSuccess = 1,
            SimIkresultFail = 2,
        }

        // Scene object sub-types
        public enum SceneObjectSubTypes
        {
            SimLightOmnidirectionalSubtype = 1,
            SimLightSpotSubtype = 2,
            SimLightDirectionalSubtype = 3,
            SimJointRevoluteSubtype = 10,
            SimJointPrismaticSubtype = 11,
            SimJointSphericalSubtype = 12,
            SimShapeSimpleshapeSubtype = 20,
            SimShapeMultishapeSubtype = 21,
            SimProximitysensorPyramidSubtype = 30,
            SimProximitysensorCylinderSubtype = 31,
            SimProximitysensorDiscSubtype = 32,
            SimProximitysensorConeSubtype = 33,
            SimProximitysensorRaySubtype = 34,
            SimMillPyramidSubtype = 40,
            SimMillCylinderSubtype = 41,
            SimMillDiscSubtype = 42,
            SimMillConeSubtype = 42,
            SimObjectNoSubtype = 200,
        }

        // Scene object main properties
        public enum SceneObjectMainProperties
        {
            SimObjectspecialpropertyCollidable = 1,
            SimObjectspecialpropertyMeasurable = 2,
            SimObjectspecialpropertyDetectableUltrasonic = 16,
            SimObjectspecialpropertyDetectableInfrared = 32,
            SimObjectspecialpropertyDetectableLaser = 64,
            SimObjectspecialpropertyDetectableInductive = 128,
            SimObjectspecialpropertyDetectableCapacitive = 256,
            SimObjectspecialpropertyRenderable = 512,
            SimObjectspecialpropertyDetectableAll = 496,
            SimObjectspecialpropertyCuttable = 1024,
            SimObjectspecialpropertyPathplanningIgnored = 2048,
        }

        // Model properties
        public enum ModelProperties
        {
            SimModelpropertyNotCollidable = 1,
            SimModelpropertyNotMeasurable = 2,
            SimModelpropertyNotRenderable = 4,
            SimModelpropertyNotDetectable = 8,
            SimModelpropertyNotCuttable = 16,
            SimModelpropertyNotDynamic = 32,
            SimModelpropertyNotRespondable = 64,
            SimModelpropertyNotReset = 128,
            SimModelpropertyNotVisible = 256,
            SimModelpropertyNotModel = 61440,
        }

        //Sim Messages
        public enum SimMessages
        {
            SimMessageUiButtonStateChange = 0,
            SimMessageReserved9 = 1,
            SimMessageObjectSelectionChanged = 2,
            SimMessageReserved10 = 3,
            SimMessageModelLoaded = 4,
            SimMessageReserved11 = 5,
            SimMessageKeypress = 6,
            SimMessageBannerclicked = 7,
            SimMessageForCApiOnlyStart = 256,
            SimMessageReserved1 = 257,
            SimMessageReserved2 = 258,
            SimMessageReserved3 = 259,
            SimMessageEventcallbackScenesave = 260,
            SimMessageEventcallbackModelsave = 261,
            SimMessageEventcallbackModuleopen = 262,
            SimMessageEventcallbackModulehandle = 263,
            SimMessageEventcallbackModuleclose = 264,
            SimMessageReserved4 = 265,
            SimMessageReserved5 = 266,
            SimMessageReserved6 = 267,
            SimMessageReserved7 = 268,
            SimMessageEventcallbackInstancepass = 269,
            SimMessageEventcallbackBroadcast = 270,
            SimMessageEventcallbackImagefilterEnumreset = 271,
            SimMessageEventcallbackImagefilterEnumerate = 272,
            SimMessageEventcallbackImagefilterAdjustparams = 273,
            SimMessageEventcallbackImagefilterReserved = 274,
            SimMessageEventcallbackImagefilterProcess = 275,
            SimMessageEventcallbackReserved1 = 276,
            SimMessageEventcallbackReserved2 = 277,
            SimMessageEventcallbackReserved3 = 278,
            SimMessageEventcallbackReserved4 = 279,
            SimMessageEventcallbackAbouttoundo = 280,
            SimMessageEventcallbackUndoperformed = 281,
            SimMessageEventcallbackAbouttoredo = 282,
            SimMessageEventcallbackRedoperformed = 283,
            SimMessageEventcallbackScripticondblclick = 284,
            SimMessageEventcallbackSimulationabouttostart = 285,
            SimMessageEventcallbackSimulationended = 286,
            SimMessageEventcallbackReserved5 = 287,
            SimMessageEventcallbackKeypress = 288,
            SimMessageEventcallbackModulehandleinsensingpart = 289,
            SimMessageEventcallbackRenderingpass = 290,
            SimMessageEventcallbackBannerclicked = 291,
            SimMessageEventcallbackMenuitemselected = 292,
            SimMessageEventcallbackRefreshdialogs = 293,
            SimMessageEventcallbackSceneloaded = 294,
            SimMessageEventcallbackModelloaded = 295,
            SimMessageEventcallbackInstanceswitch = 296,
            SimMessageEventcallbackGuipass = 297,
            SimMessageEventcallbackMainscriptabouttobecalled = 298,
            SimMessageEventcallbackRmlposition = 299,
            SimMessageEventcallbackRmlvelocity = 300,

            SimMessageSimulationStartResumeRequest = 4096,
            SimMessageSimulationPauseRequest = 4097,
            SimMessageSimulationStopRequest = 4098,
        }

        //Vrep Script call Types scriptHandleOrType
        public enum ScriptHandleOrType
        {
            sim_scripttype_mainscript = 0, //the main script will be called.
            sim_scripttype_childscript = 1,// a child script will be called.
            sim_scripttype_jointctrlcallback = 4, // a joint control callback script will be called.
            sim_scripttype_contactcallback = 5, // the contact callback script will be called.
            sim_scripttype_customizationscript = 6, //a customization script will be called.
            sim_scripttype_generalcallback = 7, //the general callback script will be called.
        }

        // Scene object properties
        public enum SceneObjectProperties
        {
            SimObjectpropertyCollapsed = 16,
            SimObjectpropertySelectable = 32,
            SimObjectpropertyReserved7 = 64,
            SimObjectpropertySelectmodelbaseinstead = 128,
            SimObjectpropertyDontshowasinsidemodel = 256,
            SimObjectpropertyCanupdatedna = 1024,
            SimObjectpropertySelectinvisible = 2048,
            SimObjectpropertyDepthinvisible = 4096,
        }

        // Remote API message header structure
        public enum MessageHeaderStructure
        {
            SimxHeaderoffsetCrc = 0,
            SimxHeaderoffsetVersion = 2,
            SimxHeaderoffsetMessageId = 3,
            SimxHeaderoffsetClientTime = 7,
            SimxHeaderoffsetServerTime = 11,
            SimxHeaderoffsetSceneId = 15,
            SimxHeaderoffsetServerState = 17,
        }

        // Remote API command header
        public enum CommandHeader
        {
            SimxCmdheaderoffsetMemSize = 0,
            SimxCmdheaderoffsetFullMemSize = 4,
            SimxCmdheaderoffsetPdataOffset0 = 8,
            SimxCmdheaderoffsetPdataOffset1 = 10,
            SimxCmdheaderoffsetCmd = 14,
            SimxCmdheaderoffsetDelayOrSplit = 18,
            SimxCmdheaderoffsetSimTime = 20,
            SimxCmdheaderoffsetStatus = 24,
            SimxCmdheaderoffsetReserved = 25,
        }

        public static float[] toFloat(double[] _double)
        {
            float[] _float = new float[_double.Length];
            for (int i = 0; i < _double.Length; i++)
            {
                float result = Convert.ToSingle(_double.GetValue(i));
                _float.SetValue(result, i);
            }
            return _float;
        }

        public static double[] toDouble(float[] _float)
        {
            double[] _double = new double[_float.Length];
            for (int i = 0; i < _float.Length; i++)
            {
                double result = Convert.ToDouble(_float.GetValue(i));
                _double.SetValue(result, i);
            }
            return _double;
        }

        public static double deg(double _rad)
        {

            double _deg = _rad * ((double)180 / Math.PI);
            return _deg;
        }

        public static double rad(double _deg)
        {
            double _rad = _deg / ((double)180 / Math.PI);
            return _rad;
        }
 
   }

    public class VRepCosy
    {
        private double x;
        private double y;
        private double z;
        private double a;
        private double b;
        private double c;
        private double[] position;
        private double[] orientation;

        public double[] Position
        {
            get { return position; }
            set
            {
                x = value[0];
                y = value[1];
                z = value[2];
            }
        }

        public double[] Orientation
        {
            get { return orientation; }
            set
            {
                a = value[0];
                b = value[1];
                c = value[2];
            }
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        public VRepCosy()
        {
            x = 0.0;
            y = 0.0;
            z = 0.0;
            a = 0.0;
            b = 0.0;
            c = 0.0;
            position = new double[] {0.0, 0.0, 0.0};
            orientation = new double[] {0.0, 0.0, 0.0};
        }

        public VRepCosy(double _x, double _y, double _z, double _a, double _b, double _c)
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
            position = new double[] {_x, _y, _z};
            orientation = new double[] {_a, _b, _c};
        }
    }

}

