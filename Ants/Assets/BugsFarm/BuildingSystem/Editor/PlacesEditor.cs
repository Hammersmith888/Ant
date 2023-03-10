using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class PlaceEditor : EditorWindow
    {
        private static PlaceEditor _window = null;
        [MenuItem("Window/Places Editor")]
        public static void Open()
        {
            _window = GetWindow<PlaceEditor>("Places Editor", true);
        }
        public List<APlace> _APlacesPrefabs;
        public List<PlaceID> _ScenePlaceIDS;
        public List<int> _freePlaceIDs;
        public Transform _placeIdParent = null;
        public PlaceID _placeIdPrefab = null;
        public uint _startFrom = 0;
    
        private const string _availablePlaceIDsRef = "Установленные PlaceIDs : ";
        private const string _drawingModeText = "Режим установки PlaceID : \n Чтобы установить PlaceID нажимайте ЛКМ";
        private const string _setupModeText = "Режим установки APlace";
        private string _availablePlaceIDs = "Установленные PlaceIDs : ";

        private bool _isinitialize = false;
        private bool _isDrawing = false;
        private bool _isSetupAPlaces = false;
        private bool _overWindow = false;
        private bool _askDoesNotParent = true;
        private const float _padding = 15f;
        private void OnGUI()
        {
            if (_window != null)
            {
                ScriptableObject target = this;
                var so = new SerializedObject(target);

                var PlaceIdParentProperty = so.FindProperty("_placeIdParent");
                var PlaceIDsProperty = so.FindProperty("_ScenePlaceIDS");
                var PlaceIdPrefabProperty = so.FindProperty("_placeIdPrefab");
                var APlacePrefabsProperty = so.FindProperty("_APlacesPrefabs");
                var StartFromProperty = so.FindProperty("_startFrom");

                _overWindow = _window == mouseOverWindow;
                if (!_isDrawing && !_isSetupAPlaces)
                {
                    GUILayout.TextArea(_availablePlaceIDs);
                    GUILayout.Space(_padding);
                    if (!_isinitialize)
                    {
                        if (GUILayout.Button("Инициализировать"))
                        {
                            var response = Init();
                            var invalide = string.IsNullOrEmpty(response);
                            _availablePlaceIDs = _availablePlaceIDsRef + (!invalide? response: " Отсутствуют на сцене !!!");
                            _isinitialize = true;
                            _window.Repaint();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Установить новые PlaceID"))
                        {
                            SceneView.duringSceneGui += SceneUpdate;
                        
                            _isDrawing = true;
                        }
                        if (GUILayout.Button("Установить новые APlace"))
                        {
                            _isSetupAPlaces = true;
                        }
                        if (GUILayout.Button("Вернуться"))
                        {
                            _isSetupAPlaces = false;
                            _isDrawing = false;
                            _isinitialize = false;
                            _availablePlaceIDs = _availablePlaceIDsRef;
                        }
                    }
                }
                else if(_isDrawing && !_isSetupAPlaces)
                {
                    GUILayout.TextArea(_drawingModeText);
                    GUILayout.Space(_padding);

                    EditorGUILayout.Space(_padding);
                    EditorGUILayout.PropertyField(StartFromProperty, true);
                    EditorGUILayout.Space(_padding);
                    EditorGUILayout.PropertyField(PlaceIdPrefabProperty, true);
                    EditorGUILayout.Space(_padding);
                    EditorGUILayout.PropertyField(PlaceIdParentProperty, true);
                    EditorGUILayout.Space(_padding);

                    if (GUILayout.Button("Применить установленные PlaceID"))
                    {
                        SceneView.duringSceneGui -= SceneUpdate;
                        _isDrawing = false;
                        _askDoesNotParent = true;
                    }
                    if (GUILayout.Button("Удалить последний PlaceID"))
                    {
                        if (_ScenePlaceIDS.Count > 0)
                        {
                            var obj = _ScenePlaceIDS[_ScenePlaceIDS.Count - 1];
                            _ScenePlaceIDS.Remove(obj);
                            if (obj != null)
                            {
                                DestroyImmediate(obj.gameObject);
                            }
                        }
                    }
                    if (GUILayout.Button("Вернуться"))
                    {
                        SceneView.duringSceneGui -= SceneUpdate;
                        _isSetupAPlaces = false;
                        _isDrawing = false;
                    }
                }
                else if(_isSetupAPlaces)
                {
                    GUILayout.TextArea(_setupModeText);
                    GUILayout.Space(_padding);


                    GUILayout.TextArea("Назначьте список PlaceIDs");
                    EditorGUILayout.PropertyField(PlaceIDsProperty, true);
                    if (GUILayout.Button("Очистить список"))
                    {
                        _ScenePlaceIDS.Clear();
                    }
                    GUILayout.Space(_padding);

                    GUILayout.TextArea("Назначьте список APlace");
                    EditorGUILayout.PropertyField(APlacePrefabsProperty, true);
                    if (GUILayout.Button("Очистить список"))
                    {
                        _APlacesPrefabs.Clear();
                    }
                    GUILayout.Space(_padding);


                    if (GUILayout.Button("Установить выбранные APlace в PlaseIDs"))
                    {
                        SetupAplaces();
                    }
                    if (GUILayout.Button("Вернуться"))
                    {
                        _isSetupAPlaces = false;
                        _isDrawing = false;
                    }
                }
                so.ApplyModifiedProperties();
            }
            else
            {
                if (MessageBox("After compilation, you need to restart the window! ", "Need to restart", DialogueType.Error))
                    Close();
            }
        }

        private void SetupAplaces()
        {
            if(_ScenePlaceIDS == null || _ScenePlaceIDS.Count == 0)
            {
                MessageBox("No link to PlaceID has been set, please set PlaceIDs.", "Set PlaceIDs", DialogueType.Error);
                return;
            }
            if(_APlacesPrefabs == null || _APlacesPrefabs.Count == 0)
            {
                MessageBox("No APlace prefab link has been installed, please install APlace prefabs.", "Set APlace Prefab", DialogueType.Error);
                return;
            }

            int numberErrors = 0;
            foreach (var prefab in _APlacesPrefabs)
            {
                foreach (var placeID in _ScenePlaceIDS)
                {
                    foreach (var aplace in placeID.GetComponentsInChildren<APlace>(true))
                    {
                        if (aplace.ModelID == prefab.ModelID)
                        {
                            Debug.LogError($"В PlaceID {placeID.PlaceNumber} : уже храниться текущий APlace {aplace.name}");
                            numberErrors++;
                        }
                    }
                }
            }

            if(numberErrors > 0)
            {
                var result = MessageBox($"Number of errors : {numberErrors}, continue creating duplicate objects?", " Contains duplicate", DialogueType.YesNo);
                if(!result)
                {
                    return;
                }
            }

            foreach (var placeID in _ScenePlaceIDS)
            {
                foreach (var prefab in _APlacesPrefabs)
                {
                    var aplace = PrefabUtility.InstantiatePrefab(prefab) as APlace;
                    aplace.transform.parent = placeID.transform;
                    aplace.transform.position = placeID.transform.position;
                }
                placeID.InitPlaces();
            }
        }
        private void SceneUpdate(SceneView view)
        {
            if (_isDrawing)
            {
                var eventCurr = Event.current;
                if (eventCurr != null && eventCurr.type == EventType.MouseDown && eventCurr.button == 0 && !_overWindow)
                {
                    if (_window != null)
                    {
                        if (_placeIdPrefab != null)
                        {
                            if (_placeIdParent == null && _askDoesNotParent)
                            {
                                var result = MessageBox("PlaceID parent is missing, do you want to continue creating PlaceID without parent? ", "Parent missing", DialogueType.YesNo);
                                if (result)
                                {
                                    _askDoesNotParent = false;
                                }
                            }
                            else
                            {
                                var placeID = (PlaceID)PrefabUtility.InstantiatePrefab(_placeIdPrefab);
                                var from = _freePlaceIDs.Contains((int)_startFrom) ? _freePlaceIDs.IndexOf((int)_startFrom) : 0;
                                placeID.transform.parent = _placeIdParent;
                                placeID.transform.position = (Vector2)HandleUtility.GUIPointToWorldRay(eventCurr.mousePosition).origin;
                                placeID.PlaceNumber = _freePlaceIDs[from].ToString();
                                placeID.name = placeID.name + "_" + placeID.PlaceNumber;
                                _ScenePlaceIDS.Add(placeID);
                                _freePlaceIDs.RemoveAt(from);
                                if(from != 0)_startFrom++;
                            }
                        }
                        else
                        {
                            MessageBox("PlaceID Prefab not found, please install!!! ", "PlaceID not found", DialogueType.Error);
                        }
                    }
                    else
                    {
                        if(MessageBox("After compilation, you need to restart the window! ", "Need to restart",DialogueType.Error))
                            Close();
                    }
                }
            }
        }
        private string Init()
        {
            _ScenePlaceIDS = new List<PlaceID>(FindObjectsOfType<PlaceID>());
            _freePlaceIDs = new List<int>();
            _APlacesPrefabs = new List<APlace>();
            var listIds = new List<int>();
            var builder = new StringBuilder();

            for (int i = 0; i < 10000; i++)
            {
                _freePlaceIDs.Add(i);
            }

            foreach (var placeID in _ScenePlaceIDS)
            {
                if(listIds.Contains(int.Parse(placeID.PlaceNumber)))
                {
                    Debug.LogError($"Такой ID уже существует : [{ placeID.PlaceNumber}] , Имя объекта :  [{placeID.name}] ");
                }
                listIds.Add(int.Parse(placeID.PlaceNumber));
          
            }
            _ScenePlaceIDS.Sort( (x1, x2) =>
            {
                if (int.Parse(x1.PlaceNumber) > int.Parse(x2.PlaceNumber))
                    return 1;
                else if (int.Parse(x1.PlaceNumber) < int.Parse(x2.PlaceNumber))
                    return -1;
                else
                    return 0;
            });
            listIds.Sort();
            var match = false;
            var idSearch = 0;
            var SequenceLength = 0;

            for (int i = 0; i < listIds.Count; i++)
            {
                var idCurr = listIds[i];
                var isLastCycles = i == (listIds.Count - 1);

                if (!match)
                {
                    builder.Append("\n");
                    builder.Append(idCurr); // Добавляем первую цифру в последовательности

                    idSearch = idCurr;
                    SequenceLength = 0;
                    match = true;
                }

                var isSequence = idCurr == idSearch;
                _freePlaceIDs.Remove(idCurr);
                if (!isLastCycles) //если это не последний цикл
                {
                    if (isSequence) // Пропускаем пока есть последовательность 
                    {
                        idSearch++;
                        SequenceLength++;
                        continue;
                    }
                    else // Если последовательность закончилась
                    {
                        if (SequenceLength > 1)
                        {
                            builder.Append(" - ");
                            builder.Append(listIds[i - 1]);
                        }
                        --i;
                    }
                }
                else if(isLastCycles)
                {
                    if (isSequence) 
                    {
                        idSearch++;
                        SequenceLength++;

                        if (SequenceLength > 1)
                        {
                            builder.Append(" - ");
                            builder.Append(idCurr);
                        }
                    }
                    else
                    {
                        if(SequenceLength > 1)
                        {
                            builder.Append(" - ");
                            builder.Append(listIds[i - 1]);
                        }
                        builder.Append("\n");
                        builder.Append(idCurr);
                    }
                }

                match = false;
            }

            return builder.ToString();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        public static IntPtr GetWindowHandle()
        {
            return GetActiveWindow();
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern int MessageBox(IntPtr hwnd, string lpText, string lpCaption, uint uType);
        /// <summary>
        /// Shows Error alert box with OK button.
        /// </summary>
        /// <param name="text">Main alert text / content.</param>
        /// <param name="caption">Message box title.</param>
        private static bool MessageBox(string text, string caption, DialogueType dialogue)
        {
            int id = 0;
            try
            {
                uint code = 0;
                switch (dialogue)
                {
                    case DialogueType.YesNo: code = (uint)(0x00000001L);
                        break;
                    case DialogueType.Error: code = (uint)(0x00000000L);
                        break;
                }
                id = MessageBox(GetWindowHandle(), text, "PlacesEditor : " + caption, code);
            }
            catch (Exception) { }
            return id == 1 || id == 6 || id == 11;
        }
        private enum DialogueType { YesNo, Error}
    }
}
