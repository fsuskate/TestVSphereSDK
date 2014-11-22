using System;
using System.Collections.Generic;
using System.Linq;
using VMware.Vim;
using System.Collections.Specialized;
using System.IO;

namespace TestVSphereSDK
{
    class Program
    {
        private LogonInfo _logonInfo = null;
        public LogonInfo LogonInfo
        {
            get
            {
                if (_logonInfo == null)
                {
                    _logonInfo = new LogonInfo();
                }
                return _logonInfo;
            }
        }

        private VimClient _vimClient = null;
        public VimClient VimClient
        {
            get
            {
                if (_vimClient == null)
                {
                    _vimClient = new VimClient();
                    try
                    {
                        AnimateCursor(ConnectAsync(LogonInfo.URI));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                    Console.WriteLine("Connected to: " + LogonInfo.URI);
                    _vimClient.Login(LogonInfo.Username, LogonInfo.Password);
                }
                return _vimClient;
            }
        }

        public void AnimateCursor(System.Threading.Tasks.Task task)
        {
            Console.Write("Connecting ");
            ConsoleColor foreColor = Console.ForegroundColor;
            ConsoleColor backgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            //Console.BackgroundColor = ConsoleColor.Green;
            Console.CursorVisible = false;
            int index = 0;
            char[] array = { '-', '/', '|', '\\' };
            while (!task.IsCompleted)
            {
                Console.Write(array[index++]);
                Console.SetCursorPosition(11, Console.CursorTop);
                if (index > 3)
                {
                    index = 0;
                }
                System.Threading.Thread.Sleep(250);
            }
            Console.Write("\n");
            Console.CursorVisible = true;
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backgroundColor;
        }

        public async System.Threading.Tasks.Task ConnectAsync(string uri)
        {
            await System.Threading.Tasks.Task.Run(() => _vimClient.Connect(uri));           
        }

        public bool PowerOnVM(string vmName)
        {
            var virtualMachine = VimClient.Find<VirtualMachine>(searchKey: "name", searchValue: vmName);
            if (virtualMachine != null)
            {
                if (virtualMachine.Runtime.PowerState == VirtualMachinePowerState.poweredOn)
                {
                    virtualMachine.PowerOffVM();
                }
                else
                {
                    virtualMachine.PowerOnVM(null);
                }
                Console.WriteLine("Powerd on VM: " + virtualMachine.Name);
            }
            return true;
        }

        public bool SnapshotVM(string vmName)
        {
            var virtualMachine = VimClient.Find<VirtualMachine>(searchKey: "name", searchValue: vmName);
            if (virtualMachine != null)
            {
                if (virtualMachine.Runtime.PowerState == VirtualMachinePowerState.poweredOn)
                {
                    virtualMachine.PowerOffVM();
                }
                else
                {
                    virtualMachine.PowerOnVM(null);
                }
                Console.WriteLine("Powerd on VM: " + virtualMachine.Name);
            }
            return true;
        }

        public bool DisplayRootFolder()
        {
            Folder rootFolder = (Folder)VimClient.GetView(VimClient.ServiceContent.RootFolder, null);
            Console.WriteLine(rootFolder.ToString());

            var childFolders = rootFolder.ChildEntity;
            foreach (var childFolder in childFolders)
            {
                Console.WriteLine(childFolder.ToString());
            }
            return true;
        }

        public bool DisplayAllPoweredOnVMs()
        {
            var virtualMachines = VimClient.Find<IList<EntityViewBase>, VirtualMachine>("Runtime.PowerState", "PoweredOn");
            foreach (VirtualMachine virtualMachine in virtualMachines)
            {
                Console.WriteLine(virtualMachine.Name);
            }
            return true;
        }

        public bool CreateVM(string folderName, string vmName, string vmPath, string resGroupMoref)
        {
            var folder = VimClient.Find<Folder>("name", folderName);
            if (folder != null)
            {
                Console.WriteLine(folder.Name);
                VirtualMachineConfigSpec virtualMachineConfigSpec = new VirtualMachineConfigSpec();
                virtualMachineConfigSpec.Name = vmName;
                virtualMachineConfigSpec.MemoryMB = 4 * 1024;
                virtualMachineConfigSpec.NumCPUs = 1;
                virtualMachineConfigSpec.Files = new VirtualMachineFileInfo()
                {
                    VmPathName = vmPath
                };

                ManagedObjectReference resourcePoolMoref = new ManagedObjectReference();
                resourcePoolMoref.Type = "ManagedObjectReference:ResourcePool";
                resourcePoolMoref.Value = resGroupMoref;

                try
                {
                    var moref = folder.CreateVM_Task(virtualMachineConfigSpec, resourcePoolMoref, null);
                    Console.WriteLine("Created VM - {0}", moref);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }

            return true;
        }

        public bool DestroyVM(string folderName, string vmName)
        {
            var folder = VimClient.Find<Folder>("name", folderName);
            if (folder == null)
            {
                throw new ArgumentOutOfRangeException("Folder name was not correct");
            }

            var virtualMachine = VimClient.Find<VirtualMachine>(searchKey: "name", searchValue: vmName);
            if (virtualMachine == null)
            {
                throw new ArgumentOutOfRangeException("Virtual Machine name was not correct");
            }

            try
            {
                var moref = (virtualMachine as ManagedEntity).Destroy_Task();
                Console.WriteLine("Destroyed VM - {0}", moref);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
            
            return true;
        }

        public bool GetVCenterInfo()
        {
            AboutInfo aboutInfo = VimClient.ServiceContent.About;
            foreach (var pInfo in aboutInfo.GetType().GetProperties())
            {
                Console.WriteLine(pInfo.Name + " : " + pInfo.GetValue(aboutInfo, null));
            }

            return true;
        }

        public bool ReconfigureVM(string vmName)
        {
            var virtualMachine = VimClient.Find<VirtualMachine>(searchKey: "name", searchValue: vmName);
            if (virtualMachine == null)
            {
                throw new ArgumentOutOfRangeException("Virtual Machine name was not correct");
            }

            VirtualMachineConfigSpec virtualMachineConfigSpec = new VirtualMachineConfigSpec();
            virtualMachineConfigSpec.NumCPUs = 4;
            virtualMachineConfigSpec.MemoryMB = 1024;
            //virtualMachineConfigSpec. // Where is the storage???

            if (virtualMachine != null)
            {
                try
                {
                    var moref = virtualMachine.ReconfigVM_Task(virtualMachineConfigSpec);
                    Console.WriteLine("Reconfigured VM - {0}", moref);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }

            return true;
        }

        static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter an operation: ");

            string input = "";
            Program program = new Program();
            try
            {
                while ((input = Console.ReadLine()) != "X")
                {
                    switch (input)
                    {
                        case "1":
                            program.PowerOnVM("Windows 8.1");
                            break;
                        case "2":
                            program.DisplayRootFolder();
                            break;
                        case "3":
                            program.DisplayAllPoweredOnVMs();
                            break;
                        case "4":
                            program.GetVCenterInfo();
                            break;
                        case "5":
                            program.CreateVM(folderName: "Test", 
                                                vmName: "TestVM", 
                                                vmPath: @"[TEST] TestCreateVM", 
                                                resGroupMoref: "resgroup-57");
                            break;
                        case "6":
                            program.DestroyVM("Test", "TestVM");
                            break;
                        case "7":
                            program.ReconfigureVM("TestVM");
                            break;
                        default:
                            break;
                    }
                    Console.Write("Enter an operation: ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();

            return 1;
        }
    }
}
