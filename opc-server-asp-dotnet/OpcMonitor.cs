using opc_server_asp_dotnet.Models;
using OpcRcw.Comn;
using OpcRcw.Da;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
namespace opc_server_asp_dotnet
{
    public class OPCMonitor : IOPCDataCallback
    {
        IOPCServer pIOPCServer;
        IOPCAsyncIO2 pIOPCAsyncIO2 = null;
        IOPCGroupStateMgt pIOPCGroupStateMgt = null;
        IConnectionPointContainer pIConnectionPointContainer = null;
        IConnectionPoint pIConnectionPoint = null;

        internal const int LOCALE_ID = 0x409;

        Object pobjGroup1 = null;
        int pSvrGroupHandle = 0;
        int nTransactionID = 0;
        int[] ItemSvrHandleArray;
        Int32 dwCookie = 0;

        int CarToatal = 2;
        int ItemTotal;
        int[] Results;
        OPCITEMDEF[] OpcItems;
        public CarOpcModel[] CarOpcModels;


        private static readonly Lazy<OPCMonitor> lazy = new Lazy<OPCMonitor>(() => new OPCMonitor());

        public static OPCMonitor Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public OPCMonitor()
        {
            CarOpcModels = new CarOpcModel[CarToatal];
            ItemTotal = CarToatal * 14 + 1;

            GCHandle hTimeBias, hDeadband;
            float deadband = 0;
            int TimeBias = 0;
            hTimeBias = GCHandle.Alloc(TimeBias, GCHandleType.Pinned);
            hDeadband = GCHandle.Alloc(deadband, GCHandleType.Pinned);

            Int32 dwRequestedUpdateRate = 250;
            Int32 hClientGroup = 1;
            Int32 pRevUpdateRate;

            Type opcType = Type.GetTypeFromProgID("OPC.SimaticNET");
            Guid iidRequiredInterface = typeof(IOPCItemMgt).GUID;

            try
            {
                pIOPCServer = (IOPCServer)Activator.CreateInstance(opcType);
                try
                {
                    pIOPCServer.AddGroup(
                        "OPC_Read_data",
                        0,
                        dwRequestedUpdateRate,
                        hClientGroup,
                        hTimeBias.AddrOfPinnedObject(),
                        hDeadband.AddrOfPinnedObject(),
                        LOCALE_ID,
                        out pSvrGroupHandle,
                        out pRevUpdateRate,
                        ref iidRequiredInterface,
                        out pobjGroup1
                    );
                    InitReqIOInterfaces();


                    OpcItems = new OPCITEMDEF[ItemTotal];
                    Results = new int[ItemTotal];

                    OpcItems[0].szAccessPath = "";                   // Accesspath not needed for this sample
                    InitOpcItems();


                    IntPtr pResults = IntPtr.Zero;
                    IntPtr pErrors = IntPtr.Zero;

                    try
                    {
                        ((IOPCItemMgt)pobjGroup1).AddItems(ItemTotal, OpcItems, out pResults, out pErrors);
                        ItemSvrHandleArray = new int[ItemTotal];

                        int[] errors = new int[ItemTotal];
                        IntPtr pos = pResults;
                        Results = new int[ItemTotal];
                        Marshal.Copy(pErrors, errors, 0, ItemTotal);

                        for (int i = 1; i < ItemTotal; i++)
                        {
                            #region 判断error[i]
                            if (errors[i] == 0)
                            {
                                pos = new IntPtr(pos.ToInt32() + Marshal.SizeOf(typeof(OPCITEMRESULT)));
                                OPCITEMRESULT result = (OPCITEMRESULT)Marshal.PtrToStructure(pos, typeof(OPCITEMRESULT));
                                Results[i] = result.hServer;
                            }
                            else
                            {
                                //logstring = "[error]：创建Item[" + i.ToString() + "]的时候失败.";
                                //OPC_main_form.log_string.Add(logstring);
                                ////LogHelper.WriteLog_error(typeof(main_form), logstring);
                            }
                            Marshal.DestroyStructure(pos, typeof(OPCITEMRESULT));
                            #endregion
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);

                    }
                    finally
                    {
                        // Free the unmanaged COM memory
                        if (pResults != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(pResults);
                            pResults = IntPtr.Zero;
                        }
                        if (pErrors != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(pErrors);
                            pErrors = IntPtr.Zero;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex);

                }
                finally
                {
                    if (hDeadband.IsAllocated) hDeadband.Free();
                    if (hTimeBias.IsAllocated) hTimeBias.Free();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);

            }
        }

        private void InitOpcItems()
        {
            OpcItems[0].szAccessPath = "";
            OpcItems[0].bActive = 1;                    // item is active
            OpcItems[0].hClient = 1;                    // client handle
            OpcItems[0].dwBlobSize = 0;                    // blob size
            OpcItems[0].pBlob = IntPtr.Zero;          // pointer to blob
            OpcItems[0].szItemID = "S7:[S7_Connection_1]DB40,X0.0";          // Item ID,
            OpcItems[0].vtRequestedDataType = 11;
            for (int i = 0; i < CarToatal; i++)
            {
                CarOpcModel carOpcModel = new CarOpcModel();
                for (int itemIndex = 0; itemIndex < 14; itemIndex++)
                {
                    int index = i * 14 + itemIndex + 1;
                    OpcItems[index].szAccessPath = "";
                    OpcItems[index].bActive = 1;                    // item is active
                    OpcItems[index].hClient = index + 1;                    // client handle
                    OpcItems[index].dwBlobSize = 0;                    // blob size
                    OpcItems[index].pBlob = IntPtr.Zero;          // pointer to blob
                    string szItemID;
                    short vtRequestedDataType;
                    if (itemIndex < 5)
                    {
                        szItemID = string.Format("S7:[S7_Connection_1]DB40,X{0}.{1}", i * 20, itemIndex);
                        vtRequestedDataType = 11;
                    }
                    else
                    {
                        szItemID = string.Format("S7:[S7_Connection_1]DB40,INT{0}", i * 20 + (itemIndex - 4) * 2);
                        vtRequestedDataType = 2;
                    }
                    OpcItems[index].szItemID = szItemID;          // Item ID,
                    OpcItems[index].vtRequestedDataType = vtRequestedDataType;                    // return values in native (cannonical) datatype
                    switch (itemIndex)
                    {
                        case 0: carOpcModel.CarActivated = new Item(index); break;
                        case 1: carOpcModel.SortActivated = new Item(index); break;
                        case 2: carOpcModel.Miding = new Item(index); break;
                        case 3: carOpcModel.BadCar = new Item(index); break;
                        case 4: carOpcModel.Sorting = new Item(index); break;
                        case 5: carOpcModel.CurrentLocation = new Item(index); break;
                        case 6: carOpcModel.Destination = new Item(index); break;
                        case 7: carOpcModel.Direction = new Item(index); break;
                        case 8: carOpcModel.RunClock = new Item(index); break;
                        case 9: carOpcModel.CarNo = new Item(index); break;
                        case 10: carOpcModel.RemainingClock = new Item(index); break;
                        case 11: carOpcModel.Size = new Item(index); break;
                        case 12: carOpcModel.X = new Item(index); break;
                        case 13: carOpcModel.Y = new Item(index); break;
                    }
                }
                CarOpcModels[i] = carOpcModel;
            }
        }
        #region OPC调用此函数来初始化异步IO接口指针
        private void InitReqIOInterfaces()
        {
            try
            {
                //查询租对象的异步调用接口
                pIOPCAsyncIO2 = (IOPCAsyncIO2)pobjGroup1;
                pIOPCGroupStateMgt = (IOPCGroupStateMgt)pobjGroup1;
                //组对象的异步调用接口
                pIConnectionPointContainer = (IConnectionPointContainer)pobjGroup1;
                //为异步建立回调
                Guid iid = typeof(IOPCDataCallback).GUID;
                pIConnectionPointContainer.FindConnectionPoint(ref iid, out pIConnectionPoint);
                //在OPC服务器的连接点和该客户端的接收器（回调对象）之间创建一个连接
                pIConnectionPoint.Advise(this, out dwCookie);
            }
            catch (System.Exception error)
            {
                Console.Write(error);
            }
        }

        #endregion

        public void Read()
        {
            int nCancelid;
            IntPtr pErrors = IntPtr.Zero;
            if (pIOPCAsyncIO2 != null)
            {
                try
                {
                    pIOPCAsyncIO2.Read(ItemTotal, Results, nTransactionID + 1, out nCancelid, out pErrors);
                    int[] errors = new int[ItemTotal];
                    Marshal.Copy(pErrors, errors, 0, ItemTotal);
                    foreach (var err in errors)
                    {
                        if (errors[0] != 0)
                        {
                            String pstrError;
                            pIOPCServer.GetErrorString(errors[0], LOCALE_ID, out pstrError);
                            Console.WriteLine(pstrError);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public void OnDataChange(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount, int[] phClientItems, object[] pvValues, short[] pwQualities, OpcRcw.Da.FILETIME[] pftTimeStamps, int[] pErrors)
        {
            throw new NotImplementedException();
        }

        public virtual void OnReadComplete(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount, int[] phClientItems, object[] pvValues, short[] pwQualities, OpcRcw.Da.FILETIME[] pftTimeStamps, int[] pErrors)
        {
            for (int i = 0; i < CarToatal; i++)
            {
                var carOpcModel = CarOpcModels[i];
                for (int itemIndex = 0; itemIndex < 14; itemIndex++)
                {

                    int index = i * 14 + itemIndex;
                    switch (itemIndex)
                    {
                        case 0: carOpcModel.CarActivated.Value = pvValues[index]; break;
                        case 1: carOpcModel.SortActivated.Value = pvValues[index]; break;
                        case 2: carOpcModel.Miding.Value = pvValues[index]; break;
                        case 3: carOpcModel.BadCar.Value = pvValues[index]; break;
                        case 4: carOpcModel.Sorting.Value = pvValues[index]; break;
                        case 5: carOpcModel.CurrentLocation.Value = pvValues[index]; break;
                        case 6: carOpcModel.Destination.Value = pvValues[index]; break;
                        case 7: carOpcModel.Direction.Value = pvValues[index]; break;
                        case 8: carOpcModel.RunClock.Value = pvValues[index]; break;
                        case 9: carOpcModel.CarNo.Value = pvValues[index]; break;
                        case 10: carOpcModel.RemainingClock.Value = pvValues[index]; break;
                        case 11: carOpcModel.Size.Value = pvValues[index]; break;
                        case 12: carOpcModel.X.Value = pvValues[index]; break;
                        case 13: carOpcModel.Y.Value = pvValues[index]; break;
                    }
                }
            }
        }

        public virtual void OnWriteComplete(int dwTransid, int hGroup, int hrMastererr, int dwCount, int[] pClienthandles, int[] pErrors)
        {
            string strResults = "";
            pIOPCServer.GetErrorString(pErrors[0], LOCALE_ID, out strResults);
        }

        public virtual void OnCancelComplete(int dwTransid, int hGroup)
        {
            throw new NotImplementedException();
        }
        public void WriteCarStatus(int CarNumber , string Status)
        {
            CarOpcModel car = CarOpcModels[CarNumber];
            int StatusIndex = car.CarActivated.Id;
            Write(StatusIndex, Status);
        }

        private void Write(int index, string status)
        {
            int[] my_ItemSvrHandleArray = new int[1];
            object[] values = new object[1];
            my_ItemSvrHandleArray[0] = Results[index];
            values[0] = status;
            //opc_wite_data_call_function(my_ItemSvrHandleArray, values);

            #region 写OPC数据部分
            int nCancelid;
            IntPtr pErrors = IntPtr.Zero;
            //object[] values = new object[1];
            //values[0] = "True";
            if (pIOPCAsyncIO2 != null)
            {
                try
                {   // Async write
                    //int[] my_new = new int[1];
                    //my_new[0] = ItemSvrHandleArray[my_opc_system_data_ctrl_Alarm_data_data_value[0].electric_motor_overload_id];
                    pIOPCAsyncIO2.Write(values.Length, my_ItemSvrHandleArray, values, values.Length, out nCancelid, out pErrors);
                    int[] errors = new int[values.Length];
                    Marshal.Copy(pErrors, errors, 0, values.Length);
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (errors[i] != 0)
                        {
                            System.Exception ex = new Exception("Error in reading item");
                            throw ex;
                        }
                    }
                }
                #region catch和finally部分
                catch (System.Exception error)
                {
                    /*MessageBox.Show(error.Message,
                        "Result-Async Read", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                }
                finally
                {
                    if (pErrors != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pErrors);
                        pErrors = IntPtr.Zero;
                    }
                }
                #endregion
            }
            #endregion
        }
    }
}
