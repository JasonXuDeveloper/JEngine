using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections;
using JEngine.Core;

namespace JEngine.Redis
{
    public enum Language
    {
        中文 = 0,
        English = 1
    }


    public class RedisWindow : EditorWindow, IHasCustomMenu
    {
        static Connection conn;//Connection实例
        static bool IsConnecting;//是否正在连接

        public static Language Language = Language.中文;

        public string SQL_IP = "127.0.0.1";//Redis 数据库IP
        public uint SQL_Port = 6379;//Redis 数据库端口
        public string SQL_Password = "";//Redis 数据库密码
        public int? SQL_DB = 0;//Redis 数据库

        #region SSH连接部分
        public bool ConnectThroughSSH = true;//是否通过SSH
        public string SSH_Host = "127.0.0.1";//SSH ip 地址
        public int SSH_Port = 22;//SSH 端口（一般情况下都是22端口）
        public string SSH_User = "root";//SSH 用户
        public string SSH_Password = "vps_password";//SSH 密码
        #endregion

        public bool Debug = true;//是否显示Log

        #region 数据部分
        public List<string> Keys;//全部Key
        public string SelectedKey;//选择的Key
        public string CurrentKey;//当前更改的Key
        public string CurrentValue;//当前更改的Value
        #endregion

        Thread thread;//子线程

        /// <summary>
        /// 初始化
        /// </summary>
        private void Awake()
        {
            //初始化值
            SQL_IP = PlayerPrefs.GetString("SQL_IP", "127.0.0.1");
            SQL_Port = (uint)PlayerPrefs.GetInt("SQL_Port", 6379);
            SQL_Password = PlayerPrefs.GetString("SQL_Password", "");
            SQL_DB = PlayerPrefs.GetInt("SQL_DB", 0);
            Debug = PlayerPrefs.GetString("Debug", "1") == "1" ? true : false;
            ConnectThroughSSH = PlayerPrefs.GetString("VIA_SSH", "1") == "1" ? true : false;
            SSH_Host = PlayerPrefs.GetString("SSH_Host", "127.0.0.1");
            SSH_Port = PlayerPrefs.GetInt("SSH_Port", 22);
            SSH_User = PlayerPrefs.GetString("SSH_User", "root");
            SSH_Password = PlayerPrefs.GetString("SSH_Password", "vps_password");
            Keys = new List<string>(0);
            CurrentValue = "";
            CurrentKey = "";
            thread = new Thread(() => { });
        }

        /// <summary>
        /// 显示Editor Window
        /// </summary>
        [MenuItem("JEngine/GUI Redis")]
        static void Init()
        {
            var window = EditorWindow.CreateWindow<RedisWindow>();
            window.titleContent = new GUIContent("GUI Redis");

            //EditorWindow.GetWindow(typeof(RedisWindow), false, "GUI Redis");
        }

        // 折叠动画
        private AnimBool fadeGroup;
        private void OnEnable()
        {
            this.fadeGroup = new AnimBool(true);
            // 注册动画监听
            this.fadeGroup.valueChanged.AddListener(this.Repaint);
            this.minSize = new Vector2(500, 300);
        }
        private void OnDisable()
        {
            // 移除动画监听
            this.fadeGroup.valueChanged.RemoveListener(this.Repaint);
        }

        private Vector2 m_ScrollPosition;//滚动视图1
        private Vector2 m_ScrollPosition2;//滚动视图2
        private Vector2 m_ScrollPosition3;//滚动视图3
        private Vector2 m_ScrollPosition4;//滚动视图4

        /// <summary>
        /// 面板GUI
        /// 这里兼容多语言，用的笨方法，以后会改善
        /// </summary>
        void OnGUI()
        {
            //GUI样式
            GUIStyle header = new GUIStyle();
            header.fontSize = 20;
            header.normal.textColor = Color.white;

            GUIStyle body = new GUIStyle();
            body.fontSize = 13;
            body.normal.textColor = Color.red;
            GUILayout.BeginVertical();
            EditorStyles.textArea.wordWrap = true;

            if (Language == Language.中文)
            {
                //全部Key区域
                GUILayout.BeginArea(new Rect(25, 10, this.position.width / 3, this.position.height - 10));

                EditorGUILayout.LabelField("Db" + SQL_DB + "中的Keys", header);
                GUILayout.Space(5);
                EditorGUILayout.LabelField("共" + Keys.Count + "Keys", body);
                if (GUILayout.Button("新增"))
                {
                    CurrentValue = "";
                    CurrentKey = "";
                    SelectedKey = "";
                    this.Repaint();
                }
                GUILayout.Space(10);

                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                foreach (var key in Keys)
                {
                    if (GUILayout.Button(key))
                    {
                        GetValue(key);
                    }
                }
                GUILayout.Space(10);
                GUILayout.EndScrollView();
                GUILayout.EndArea();


                //配置+数据区域
                GUILayout.BeginArea(new Rect(this.position.width / 3 + 50, 10, this.position.width / 3 * 2 - 75, this.position.height - 110));
                EditorGUILayout.HelpBox("Unity GUI Redis\n" +
               "作者: 傑", MessageType.Info);
                Language = (Language)EditorGUILayout.EnumPopup("语言(Language)", Language);
                //配置区域
                m_ScrollPosition2 = EditorGUILayout.BeginScrollView(m_ScrollPosition2);
                this.fadeGroup.target = EditorGUILayout.Foldout(this.fadeGroup.target, "基础设置", true);
                if (EditorGUILayout.BeginFadeGroup(this.fadeGroup.faded))
                {
                    EditorGUILayout.HelpBox("注意:\n如果Redis服务器外网没访问权，请使用SSH连接\n如果通过SSH连接，数据库IP为Redis配置文件中绑定的地址（理论上为127.0.0.1）\n如果不通过SSH连接，数据库IP为服务器IP\nRedis数据库端口默认是6379\n数据库如果没密码就留空\n数据库默认为0", MessageType.Warning);
                    SQL_IP = EditorGUILayout.TextField("数据库IP", SQL_IP);
                    SQL_Port = (uint)EditorGUILayout.IntField("数据库端口", (int)SQL_Port);
                    SQL_Password = EditorGUILayout.PasswordField("数据库密码", SQL_Password);
                    SQL_DB = EditorGUILayout.IntField("数据库", (int)SQL_DB);
                    Debug = EditorGUILayout.Toggle("调试模式", Debug);
                    GUILayout.Space(10);
                    ConnectThroughSSH = EditorGUILayout.BeginToggleGroup("使用SSH连接（建议开启）", ConnectThroughSSH);
                    EditorGUILayout.HelpBox("注意:\nSSH服务器必须具有数据库访问权（即数据库在该服务器内或该服务器IP有权限访问数据库）\nSSH端口默认22\nSSH用户为服务器登入用户名，CentOS的机子一般是root，Ubuntu机子是ubuntu，Windows机子是Administrator\nSSH密码为你登入服务器的密码", MessageType.Warning);
                    SSH_Host = EditorGUILayout.TextField("SSH服务器IP", SSH_Host);
                    SSH_Port = EditorGUILayout.IntField("端口", SSH_Port);
                    SSH_User = EditorGUILayout.TextField("用户", SSH_User);
                    SSH_Password = EditorGUILayout.PasswordField("密码", SSH_Password);
                    EditorGUILayout.EndToggleGroup();
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndFadeGroup();
                //数据处理区域
                GUILayout.Space(5);
                CurrentKey = EditorGUILayout.TextField("Key", CurrentKey);
                EditorGUILayout.LabelField("Value");
                m_ScrollPosition3 = EditorGUILayout.BeginScrollView(m_ScrollPosition3);
                CurrentValue = EditorGUILayout.TextArea(CurrentValue, GUILayout.MinHeight(150), GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if (CurrentKey != "" && CurrentValue != "" && SelectedKey!= "")
                {
                    if (GUILayout.Button("保存"))
                    {
                        SetValue();
                    }
                    if (GUILayout.Button("删除"))
                    {
                        DeleteValue();
                    }
                }
                else
                {
                    if (GUILayout.Button("新增"))
                    {
                        SetValue();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                //按钮区域
                GUILayout.BeginArea(new Rect(this.position.width / 3 + 25 + ((this.position.width / 3 * 2) - 275) / 2, this.position.height - 90, 250, 80));
                m_ScrollPosition4 = EditorGUILayout.BeginScrollView(m_ScrollPosition4);
                if (GUILayout.Button("刷新Keys"))
                {
                    GetKeys();
                }
                if (GUILayout.Button("测试连接"))
                {
                    TestConnection();
                }
                EditorGUILayout.HelpBox("测试连接会阻塞主线程，建议不要在游戏运行时点击测试，慎用！", MessageType.Error);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                EditorGUILayout.EndVertical();
            }

            #region 如果不需要英文版可以把下面的删了

            else//英文版本
            {
                //全部Key区域
                GUILayout.BeginArea(new Rect(25, 10, this.position.width / 3, this.position.height - 10));

                EditorGUILayout.LabelField("Keys in Db" + SQL_DB + "", header);
                GUILayout.Space(5);
                EditorGUILayout.LabelField(Keys.Count + " Keys in total", body);
                if (GUILayout.Button("Add new key"))
                {
                    CurrentValue = "";
                    CurrentKey = "";
                    this.Repaint();
                }
                GUILayout.Space(10);

                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                foreach (var key in Keys)
                {
                    if (GUILayout.Button(key))
                    {
                        GetValue(key);
                    }
                }
                GUILayout.Space(10);
                GUILayout.EndScrollView();
                GUILayout.EndArea();


                //配置+数据区域
                GUILayout.BeginArea(new Rect(this.position.width / 3 + 50, 10, this.position.width / 3 * 2 - 75, this.position.height - 110));
                EditorGUILayout.HelpBox("Unity GUI Redis\n" +
                "Author: JasonXuDeveloper", MessageType.Info);
                Language = (Language)EditorGUILayout.EnumPopup("语言(Language)", Language);
                //配置区域
                m_ScrollPosition2 = EditorGUILayout.BeginScrollView(m_ScrollPosition2);
                this.fadeGroup.target = EditorGUILayout.Foldout(this.fadeGroup.target, "Basics", true);
                if (EditorGUILayout.BeginFadeGroup(this.fadeGroup.faded))
                {
                    EditorGUILayout.HelpBox("Note:\nPlease use SSH if your Redis service refuses to connect from your ip\nWhen you are using SSH, please change the SQL IP to bind IP in your redis configuration (usually is 127.0.0.1)\nIf you are not using SSH, change the SQL IP to you Server IP\nDefault port for Redis is 6379\nRemain SQL Password empty when there is no password\nDefault database is 9", MessageType.Warning);
                    SQL_IP = EditorGUILayout.TextField("SQL IP", SQL_IP);
                    SQL_Port = (uint)EditorGUILayout.IntField("SQL Port", (int)SQL_Port);
                    SQL_Password = EditorGUILayout.PasswordField("SQL Password", SQL_Password);
                    SQL_DB = EditorGUILayout.IntField("Database", (int)SQL_DB);
                    Debug = EditorGUILayout.Toggle("Debug mode", Debug);
                    GUILayout.Space(10);
                    ConnectThroughSSH = EditorGUILayout.BeginToggleGroup("Use SSH (Highly recommended)", ConnectThroughSSH);
                    EditorGUILayout.HelpBox("Note:\nSSH must be avaliable to connect to Redis\nDefault port for SSH is 22\nSSH Username is usually root on CentOS Machines, ubuntu on Ubuntu Machines, Administrator on Windows\nSSH Password is the password which you access to your machine", MessageType.Warning);
                    SSH_Host = EditorGUILayout.TextField("SSH IP", SSH_Host);
                    SSH_Port = EditorGUILayout.IntField("SSH Port", SSH_Port);
                    SSH_User = EditorGUILayout.TextField("SSH User", SSH_User);
                    SSH_Password = EditorGUILayout.PasswordField("SSH Password", SSH_Password);
                    EditorGUILayout.EndToggleGroup();
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndFadeGroup();
                //数据处理区域
                GUILayout.Space(5);
                CurrentKey = EditorGUILayout.TextField("Key", CurrentKey);
                EditorGUILayout.LabelField("Value");
                m_ScrollPosition3 = EditorGUILayout.BeginScrollView(m_ScrollPosition3);
                CurrentValue = EditorGUILayout.TextArea(CurrentValue, GUILayout.MinHeight(150), GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if (CurrentKey != "" && CurrentValue != "")
                {
                    if (GUILayout.Button("Save"))
                    {
                        SetValue();
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        DeleteValue();
                    }
                }
                else
                {
                    if (GUILayout.Button("Add New"))
                    {
                        SetValue();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                //按钮区域
                GUILayout.BeginArea(new Rect(this.position.width / 3 + 25 + ((this.position.width / 3 * 2) - 275) / 2, this.position.height - 90, 250, 80));
                m_ScrollPosition4 = EditorGUILayout.BeginScrollView(m_ScrollPosition4);
                if (GUILayout.Button("Refresh Keys"))
                {
                    GetKeys();
                }
                if (GUILayout.Button("Test Connection"))
                {
                    TestConnection();
                }
                EditorGUILayout.HelpBox("Test connection will be running on main thread,\nThus it is not recommended to try it while the game is playing,\nPlease be aware before you use this feature", MessageType.Error);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                EditorGUILayout.EndVertical();
            }
            #endregion
            //删到这里就够了
        }

        int? LastCount;//自动更新视图
        string LastValue;//上一个值

        /// <summary>
        /// 动态监测
        /// </summary>
        private void Update()
        {
            //30帧监测一次
            if (Application.targetFrameRate % 30 != 0)
            {
                return;
            }

            if (thread == null)
            {
                thread = new Thread(() => { });
            }

            if (LastValue == null)
            {
                LastValue = "";
            }

            if (LastCount == null)
            {
                LastCount = 0;
            }

            if (LastCount != Keys.Count)
            {
                this.Repaint();
                LastCount = Keys.Count;
            }
            if (LastValue != CurrentValue)
            {
                this.Repaint();
                LastValue = CurrentValue;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void InitConnection()
        {
            if (thread == null)
            {
                thread = new Thread(() => { });
            }
            thread.Abort();
            conn = new Connection
            {
                SQL_IP = SQL_IP,
                SQL_Port = SQL_Port,
                SQL_Password = SQL_Password,
                SQL_DB = SQL_DB == null ? 0 : (int)SQL_DB,
                ConnectThroughSSH = ConnectThroughSSH,
                SSH_Host = SSH_Host,
                SSH_Port = SSH_Port,
                SSH_User = SSH_User,
                SSH_Password = SSH_Password,
                Debug = Debug
            };
            PlayerPrefs.SetString("SQL_IP", SQL_IP);
            PlayerPrefs.SetInt("SQL_Port", (int)SQL_Port);
            PlayerPrefs.SetString("SQL_Password", SQL_Password);
            PlayerPrefs.SetInt("SQL_DB", (int)SQL_DB);
            PlayerPrefs.SetString("VIA_SSH", ConnectThroughSSH ? "1" : "0");
            PlayerPrefs.SetString("SSH_Host", SSH_Host);
            PlayerPrefs.SetInt("SSH_Port", SSH_Port);
            PlayerPrefs.SetString("SSH_User", SSH_User);
            PlayerPrefs.SetString("SSH_Password", SSH_Password);
            PlayerPrefs.SetString("Debug", Debug ? "1" : "0");

            GUI.FocusControl(null);
        }


        /// <summary>
        /// 测试连接
        /// 因为弹窗不能跑子线程，该方法会阻塞主线程，慎用
        /// </summary>
        void TestConnection()
        {
            InitConnection();

            if (IsConnecting)
            {
                if (Language == Language.中文)
                {
                    Log.PrintError("连接正在进行");
                }
                else
                {
                    Log.PrintError("It is currently connecting the server");
                }
                return;
            }
            IsConnecting = true;
            Keys = new List<string>(0);
            CurrentValue = "";
            CurrentKey = "";

            //开启计时器
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            conn.Connect(
               redis =>
               {
                   stopwatch.Stop();
                   IsConnecting = false;
                   if (Language == Language.中文)
                   {
                       EditorUtility.DisplayDialog("UNITY GUI Redis", "连接Redis数据库成功，共耗时" + stopwatch.ElapsedMilliseconds + "ms", "关闭窗口");
                   }
                   else
                   {
                       EditorUtility.DisplayDialog("UNITY GUI Redis", "Connection Success, it takes " + stopwatch.ElapsedMilliseconds + "ms", "Close tab");
                   }
               },
               () =>
               {
                   stopwatch.Stop();
                   IsConnecting = false;
                   if (Language == Language.中文)
                   {
                       EditorUtility.DisplayDialog("UNITY GUI Redis", "连接Redis数据库失败，连接超时", "关闭窗口");
                   }
                   else
                   {
                       EditorUtility.DisplayDialog("UNITY GUI Redis", "Connection failed, time is out", "Close tab");
                   }
               });
        }


        /// <summary>
        /// 获取全部Key
        /// </summary>
        void GetKeys()
        {
            InitConnection();

            if (IsConnecting)
            {
                if (Language == Language.中文)
                {
                    Log.PrintError("连接正在进行");
                }
                else
                {
                    Log.PrintError("It is currently connecting the server");
                }
                return;
            }
            IsConnecting = true;
            Keys = new List<string>(0);
            CurrentValue = "";
            CurrentKey = "";
            thread.Abort();
            thread = new Thread(() =>
            {
                conn.Connect(
                    redis =>
                    {
                        Keys = new List<string>(0);
                        redis.GetAllKeys().ForEach(
                            key =>
                            Keys.Add(key.ToString())
                        );
                        IsConnecting = false;
                    },
                    () =>
                    {
                        IsConnecting = false;
                    });
            });
            thread.Start();
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="Key"></param>
        void GetValue(string Key)
        {
            InitConnection();

            if (IsConnecting)
            {
                if (Language == Language.中文)
                {
                    Log.PrintError("连接正在进行");
                }
                else
                {
                    Log.PrintError("It is currently connecting the server");
                }
                return;
            }
            IsConnecting = true;
            thread.Abort();
            thread = new Thread(() =>
            {
                conn.Connect(
                    redis =>
                    {
                        CurrentKey = Key;
                        SelectedKey = Key;
                        CurrentValue = redis.GetValue(Key);
                        IsConnecting = false;
                    },
                    () =>
                    {
                        IsConnecting = false;
                    });
            });
            thread.Start();
        }

        /// <summary>
        /// 更新值
        /// </summary>
        void SetValue()
        {
            InitConnection();

            if (IsConnecting)
            {
                if (Language == Language.中文)
                {
                    Log.PrintError("连接正在进行");
                }
                else
                {
                    Log.PrintError("It is currently connecting the server");
                }
                return;
            }
            IsConnecting = true;
            thread.Abort();
            thread = new Thread(() =>
            {
                conn.Connect(
                    redis =>
                    {
                        if (SelectedKey != null && SelectedKey != "")
                        {
                            redis.Del(SelectedKey);
                        }

                        SelectedKey = CurrentKey;
                        redis.SetValue(CurrentKey, CurrentValue);
                        if (Language == Language.中文)
                        {
                            Log.Print("保存成功");
                        }
                        else
                        {
                            Log.Print("Successfully saved");
                        }
                        Keys = new List<string>(0);
                        redis.GetAllKeys().ForEach(
                            key =>
                            Keys.Add(key.ToString())
                        );
                        IsConnecting = false;
                    },
                    () =>
                    {
                        IsConnecting = false;
                    });
            });
            thread.Start();
        }

        /// <summary>
        /// 删除值
        /// </summary>
        void DeleteValue()
        {
            InitConnection();

            if (IsConnecting)
            {
                if (Language == Language.中文)
                {
                    Log.PrintError("连接正在进行");
                }
                else
                {
                    Log.PrintError("It is currently connecting the server");
                }
                return;
            }
            IsConnecting = true;
            thread.Abort();
            thread = new Thread(() =>
            {
                conn.Connect(
                    redis =>
                    {
                        redis.Del(CurrentKey);
                        if (Language == Language.中文)
                        {
                            Log.Print("删除成功");
                        }
                        else
                        {
                            Log.Print("Successfully deleted");
                        }
                        Keys = new List<string>(0);
                        redis.GetAllKeys().ForEach(
                            key =>
                            Keys.Add(key.ToString())
                        );
                        CurrentKey = "";
                        CurrentValue = "";
                        IsConnecting = false;
                    },
                    () =>
                    {
                        IsConnecting = false;
                    });
            });
            thread.Start();
        }


        /// <summary>
        /// 右上角功能
        /// </summary>
        /// <param name="menu"></param>
        public void AddItemsToMenu(GenericMenu menu)
        {
            if (Language == Language.中文)
            {
                menu.AddItem(new GUIContent("清除PlayerPrefs缓存"), false, OnClean);
                menu.AddItem(new GUIContent("关于"), false, Details);
            }
            else
            {
                menu.AddItem(new GUIContent("Wipe PlayerPrefs Cache"), false, OnClean);
                menu.AddItem(new GUIContent("About"), false, Details);
            }
        }

        void Details()
        {
            if (Language == Language.中文)
            {
                Log.PrintWarning("关于插件");
                Log.Print("插件名称: GUI Redis");
                Log.Print("作者: 傑");
                Log.Print("版本: v1.0");
                Log.PrintWarning("插件功能");
                Log.Print("连接Redis数据库");
                Log.Print("查询数据");
                Log.Print("增加数据");
                Log.Print("修改数据");
                Log.Print("删除数据");
                Log.PrintError("使用须知");
                Log.PrintWarning("目前版本仅支持最基础的key-value查询");
                Log.PrintWarning("Hash, List, Set等暂不支持");
            }
            else
            {
                Log.PrintWarning("About asset");
                Log.Print("Asset name: GUI Redis");
                Log.Print("Author: JasonXuDeveloper");
                Log.Print("Version: v1.0");
                Log.PrintWarning("Features");
                Log.Print("Connect Redis Database");
                Log.Print("Get Data");
                Log.Print("Add Data");
                Log.Print("Edit Data");
                Log.Print("Delete Data");
                Log.PrintError("Notes");
                Log.PrintWarning("Current version only supports operating key-value data");
                Log.PrintWarning("Hash, List, Set are currently unavaliable");
            }
        }

        void OnClean()
        {
            PlayerPrefs.DeleteKey("SQL_IP");
            PlayerPrefs.DeleteKey("SQL_Port");
            PlayerPrefs.DeleteKey("SQL_Password");
            PlayerPrefs.DeleteKey("SQL_DB");
            PlayerPrefs.DeleteKey("VIA_SSH");
            PlayerPrefs.DeleteKey("SSH_Host");
            PlayerPrefs.DeleteKey("SSH_Port");
            PlayerPrefs.DeleteKey("SSH_User");
            PlayerPrefs.DeleteKey("SSH_Password");
            PlayerPrefs.DeleteKey("Debug");
            if (Language == Language.中文)
            {
                Log.Print("清除成功！");
            }
            else
            {
                Log.Print("Successfully wiped");
            }
        }
    }
}
