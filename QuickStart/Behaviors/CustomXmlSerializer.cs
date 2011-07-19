namespace QuickStart.Behaviors
{
    // From:  http://www.codeproject.com/KB/XML/CustomXmlSerializer.aspx

    /// <summary>
    /// Defines how to serialize class members (fields and properties)
    /// </summary>
    /// <remarks></remarks>
    public enum SerializationMethod
    {
        /// <summary>
        /// Serialize public members
        /// </summary>
        /// <remarks></remarks>
        Shallow,

        /// <summary>
        /// Serialize private, friend, and public members
        /// </summary>
        /// <remarks></remarks>
        Deep
    }


    /// <summary>
    /// Serialize and deserialize objects into and from Xml.
    /// Write operations serialize the object into various target mediums.
    /// Read operations deserialize the object from various source mediums.
    /// </summary>
    /// <remarks>
    /// Designed and Created by Larry Steinle, 2006.
    /// 
    /// Deserializing structure data types is not supported.
    /// Serialization/Deserialization of circular references is not supported.
    /// Designed for use with System.Xml.Serialization.IXmlSerializable.
    /// 
    /// Standard FreeWare Licensing Applies. This software is to be used free of charge and may not be sold.
    ///
    /// Resources:
    /// http://www.programmersheaven.com/2/Dot-Net-Reflection-Part-1-Page2
    /// Elements are used with inner text at all times. Attributes aren't supported. 
    /// This ensures that we won't have any translation problems when loading the Xml into the target object.
    /// Note: This class does not support deserializing structures.
    ///
    /// Reason Structures Aren't Supported for Deserialization: http://www.dotnet247.com/247reference/msgs/31/158508.aspx
    /// The SetValue method takes an object parameter, which causes a boxing
    /// operation. SetValue ends up being called on the heap-based boxed copy
    /// rather than the stack-based copy. You need to unbox the heap-based copy
    /// back to the stack to see the end result of the SetValue call.
    /// 
    /// FIX: December 9, 2006 - Code Changes to Correctly Manage IDictionary Object Types
    /// When serializing/deserializing classes that inherit from IDictionary the property
    /// IncludeClassNameAttribute must be set to a value of True. This is because the item
    /// property for an IDictionary class cannot be interogated for it's data type. The item
    /// property always returns a DictionaryEntry which has a value type of object.
    /// </remarks>
    public class CustomXmlSerializer
    {
        #region "Public Properties: Serialization Behavior"
        private bool m_UseCData = false;
        private bool m_IgnoreWarnings = true;
        private bool m_IncludeClassNameAttribute = false;
        private SerializationMethod m_Method = SerializationMethod.Shallow;

        /// <summary>
        /// Serialize string values into xml CData tags.
        /// </summary>
        /// <value>true to enable CData serialization, False to disable and store as string.</value>
        /// <returns>Boolean value identifing property state.</returns>
        /// <remarks>When enabled strings and enumerators are stored in CData tags.</remarks>
        public bool CDataStorage
        {
            get { return m_UseCData; }
            set { m_UseCData = value; }
        }


        /// <summary>
        /// Ignore warnings and allow operation to continue.
        /// </summary>
        /// <value>true to ignore warning errors, False to throw warning errors.</value>
        /// <returns>Boolean value identifing property state.</returns>
        /// <remarks>Use with caution as deserialization can load objects with incomplete data.</remarks>
        public bool IgnoreWarnings
        {
            get { return m_IgnoreWarnings; }
            set { m_IgnoreWarnings = value; }
        }


        /// <summary>
        /// Record the name of the class when serializing to ensure that the
        /// class can be deserialized.
        /// </summary>
        /// <value>true to include the className, False to exclude it.</value>
        /// <returns>The state of the property.</returns>
        /// <remarks></remarks>
        public bool IncludeClassNameAttribute
        {
            get { return m_IncludeClassNameAttribute; }
            set { m_IncludeClassNameAttribute = value; }
        }


        /// <summary>
        /// Identifies how the class should be serialized.
        /// </summary>
        /// <value>Shallow to serialize public fields and properties. Deep to serialize private, friend, and public fields and properties.</value>
        /// <returns>The state of the property.</returns>
        /// <remarks></remarks>
        public SerializationMethod Method
        {
            get { return m_Method; }
            set { m_Method = value; }
        }


        /// <summary>
        /// Defines theSystem.Reflection BindingFlags required to support the selected SerializationMethod.
        /// </summary>
        /// <returns>The state of the property.</returns>
        /// <remarks></remarks>
        private System.Reflection.BindingFlags BindingCriteria
        {
            get
            {
                //FEATURE: June 22, 2006 - Added to support new Method property.
                System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
                if (Method == SerializationMethod.Deep)
                {
                    Flags = Flags | System.Reflection.BindingFlags.NonPublic;
                }
                return Flags;
            }
        }
        #endregion

        #region "Public Methods: Deserialization Routines"
        /// <summary>
        /// Deserialize Xml into the target object.
        /// </summary>
        /// <param name="reader">The source of the Xml to load.</param>
        /// <param name="target">The destination for the Xml.</param>
        /// <remarks>
        /// The target must be passed in ByVal and returned to support data type variable serialization.
        /// </remarks>
        public object ReadXml(System.Xml.XmlReader reader, object target)
        {
            //Get Attributes to Identify Type of Object to Save
            System.Collections.SortedList _MetaInstructions = GetAttributes(reader);
            string _ClassName = string.Empty;

            if (!(_MetaInstructions == null) & (_MetaInstructions.ContainsKey("className")))
            {
                _ClassName = System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("className")));
            }

            MoveToRootNode(reader);

            //Attempt to Identify Target Type
            if (reader.IsEmptyElement)
            {
                //No data to work with for the current element. Continue processing.
            }
            else if (target is System.Array)
            {
                //Feature: Added Support for System.Array
                target = ReadArray(reader, target);
            }
            else if (IsDataType(target) | IsDataType(reader))
            {	//Since all values are stored as string in Xml we 
                //have to cast the variable back to its original type.
                //Data types are stored with their type as the node name.
                string _DataType = reader.Name;

                MoveToValueNode(reader);

                SaveValue(ref target, _DataType, reader.Value);
                MoveToNextTag(reader);
            }
            else if (IsDataType(_ClassName, true))
            {
                //FIX: December 9, 2006 - Add support when data type is recorded in XML.
                //Since all values are stored as string in Xml we 
                //have to cast the variable back to its original type.
                //Data types are stored with their type as the node name.
                string _DataType = System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("className")));
                MoveToValueNode(reader);
                SaveValue(ref target, _DataType, reader.Value);
                MoveToNextTag(reader);
            }
            else if (target is System.Enum)
            {
                MoveToValueNode(reader);
                target = System.Enum.Parse(target.GetType(), reader.Value);
                MoveToNextTag(reader);
            }
            else if (target is System.Array)
            {
                //Feature: Added Support for System.Array
                target = ReadArray(reader, target);
            }
            else if (target == null)
            {
                throw new System.InvalidOperationException("Unable to deserialize Xml into " + target.GetType().FullName + ". The target parameter must be initialized.");
            }
            else
            {
                MoveToNextNode(reader);

                while ((reader.NodeType != System.Xml.XmlNodeType.EndElement)
                    & (string.Compare(reader.Name, "System.Collections.IEnumerable", true) != 0))
                {
                    if (reader.IsEmptyElement)
                    {
                        //No data to work with for the current element. Continue processing.
                    }
                    else
                    {
                        target = ReadFields(reader, target);
                        target = ReadProperties(reader, target);
                    }

                    MoveToNextTag(reader);

                }

                //FIX: July 13, 2006 - Support the scenario where the Item of an IEnumerable is itself an IEnumerable.
                if (target is System.Collections.IEnumerable)
                {
                    while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        target = ReadChildren(reader, target, target);
                        MoveToNextTag(reader);
                    }
                }
            }

            return target;
        }


        /// <summary>
        /// Deserialize Xml into the target object.
        /// </summary>
        /// <param name="node">The Xml to load into the object.</param>
        /// <param name="target">The destination for the Xml.</param>
        /// <remarks></remarks>
        public object ReadXml(System.Xml.XmlNode node, object target)
        {
            System.Xml.XmlNodeReader _xmlReader = new System.Xml.XmlNodeReader(node);
            target = ReadXml(_xmlReader, target);
            return target;
        }


        /// <summary>
        /// Deserialize Xml into the target object.
        /// </summary>
        /// <param name="document">The Xml to load into the object.</param>
        /// <param name="target">The destination for the Xml.</param>
        /// <remarks></remarks>
        public object ReadXml(System.Xml.XmlDocument document, object target)
        {
            System.Xml.XmlNodeReader _xmlReader = new System.Xml.XmlNodeReader(document);
            target = ReadXml(_xmlReader, target);
            return target;
        }


        /// <summary>
        /// Deserialize Xml into the target object.
        /// </summary>
        /// <param name="path">A path to the file with the Xml to load into the object.</param>
        /// <param name="target">The destination for the Xml.</param>
        /// <remarks></remarks>
        public object ReadXml(string path, object target)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader _File = new System.IO.StreamReader(path);
                System.Text.StringBuilder _xmlText = new System.Text.StringBuilder();

                _xmlText.Append(_File.ReadToEnd());
                _File.Close();

                target = ReadXml(_xmlText, target);
                return target;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Deserialize Xml into the target object.
        /// </summary>
        /// <param name="text">The Xml to load into the object.</param>
        /// <param name="target">The destination for the Xml.</param>
        /// <remarks></remarks>
        public object ReadXml(System.Text.StringBuilder text, object target)
        {
            System.IO.StringReader _textStreamReader = new System.IO.StringReader(text.ToString());
            System.Xml.XmlTextReader _xmlReader = new System.Xml.XmlTextReader(_textStreamReader);
            target = ReadXml(_xmlReader, target);
            return target;
        }
        #endregion

        #region "Public Methods: Serialization Routines"
        /// <summary>
        /// Serialize the source object into an XmlDocument following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        /// <remarks></remarks>
        public System.Xml.XmlDocument WriteDocument(object source)
        {
            System.Xml.XmlDocument _xmlDoc = new System.Xml.XmlDocument();
            _xmlDoc.LoadXml(WriteString(source));
            return _xmlDoc;
        }


        /// <summary>
        /// Serialize the source object into a file following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="path">The file to save the Xml into.</param>
        /// <remarks>
        /// If the file exists serialization is terminated.
        /// </remarks>
        public void WriteFile(object source, string path)
        {
            WriteFile(source, path, false);
        }


        /// <summary>
        /// Serialize the source object into a file following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="path">The file to save the Xml into.</param>
        /// <param name="replaceFile">
        /// If true the file is deleted before the contents are saved.
        /// If false and the file exists serialization is terminated.
        /// </param>
        /// <remarks></remarks>
        public void WriteFile(object source, string path, bool replaceFile)
        {
            if (replaceFile & System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            if (!(System.IO.File.Exists(path)))
            {
                string _xmlText = WriteString(source);
                System.IO.StreamWriter _File = new System.IO.StreamWriter(path);
                _File.Write(_xmlText);
                _File.Close();
            }
        }


        /// <summary>
        /// Serialize the source object into a string following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        /// <remarks></remarks>
        public string WriteString(object source)
        {
            return WriteText(source).ToString();
        }


        /// <summary>
        /// Serialize the source object into an StringBuilder following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        /// <remarks></remarks>
        public System.Text.StringBuilder WriteText(object source)
        {
            System.Text.StringBuilder _xmlText = new System.Text.StringBuilder();

            //Serialize the class to Xml
            System.IO.StringWriter _TextStreamWriter = new System.IO.StringWriter(_xmlText);
            System.Xml.XmlTextWriter _xmlWriter = new System.Xml.XmlTextWriter(_TextStreamWriter);
            WriteXml(source, _xmlWriter);
            _xmlWriter.Flush();
            return _xmlText;
        }


        /// <summary>
        /// Serialize the source object into an XmlWriter following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="writer">The destination for the xml.</param>
        /// <remarks>
        /// Shallow Copy means that only the exposed properties are serialized. 
        /// Hidden fields, properties, or protected properties are ignored.
        /// </remarks>
        public void WriteXml(object source, System.Xml.XmlWriter writer)
        {
            WriteXml(source, writer, null);
        }


        /// <summary>
        /// Serialize the source object into an XmlWriter following "Shallow Copy" business logic.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="writer">The destination for the xml.</param>
        /// <param name="propertyName">If serializing a class property provide the name of the property. If serializing a class then set to null.</param>
        /// <remarks>
        /// Shallow Copy means that only the exposed properties are serialized. 
        /// Hidden fields, properties, or protected properties are ignored.
        /// </remarks>
        public void WriteXml(object source, System.Xml.XmlWriter writer, string propertyName)
        {
            string _ElementName;

            if (source == null)
            { return; }
            else if (propertyName == null)
            { _ElementName = source.GetType().Name; }
            else
            {
                //Use the pre-defined name for the Xml Node Name
                _ElementName = propertyName;
            }

            if (_ElementName.IndexOf("[") > 0)
                _ElementName = _ElementName.Substring(0, _ElementName.IndexOf("["));

            writer.WriteStartElement(_ElementName);

            //Feature: Added Support for System.Array
            if (source is System.Array)
            {
                writer.WriteAttributeString("size", null, GetArraySize((System.Array)source));
                writer.WriteAttributeString("className", null, "System.Array");
                writer.WriteAttributeString("type", null, GetArrayType((System.Array)source));
            }

            WriteKey(ref source, writer);
            WriteClassName(source, writer);

            if (IsDataType(source))
            {
                if (CDataStorage & (source is System.Char | source is System.String))
                { writer.WriteCData(source.ToString().Trim()); }
                else
                { writer.WriteString(source.ToString().Trim()); }
            }
            else if (source is System.Array)
            {
                //Feature: Added Support for System.Array
                WriteArray((System.Array)source, writer);
            }
            else if (source is System.Enum)
            { writer.WriteString(source.ToString().Trim()); }
            else
            {
                WriteFields(source, writer);
                WriteProperties(source, writer);

//                //FIX: July 13, 2006 - Support the scenario where the Item of an IEnumerable is itself an IEnumerable.
//                if (source is System.Collections.IEnumerable)
//                {
//                    writer.WriteStartElement("System.Collections.IEnumerable");
//                    foreach (object _Item in (System.Collections.IEnumerable)source)
//                    {
//                        WriteXml(_Item, writer);
//                    }
//                    writer.WriteEndElement();
//                }
            }

            writer.WriteEndElement();
        }
        #endregion

        #region "Private Methods: Deserialization Helpers"
        /// <summary>
        /// Assings the Xml values to the fields.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <param name="target">The class containing the fields to update.</param>
        /// <returns>The updated class.</returns>
        /// <remarks></remarks>
        private object ReadFields(System.Xml.XmlReader reader, object target)
        {
            object _FieldValue;

            foreach (System.Reflection.FieldInfo _Field in target.GetType().GetFields(BindingCriteria))
            {
                if (string.Compare(reader.Name, _Field.Name, true) == 0)
                {
                    //BEGIN: FIX: July 13, 2006 - Check for Array Data Type
                    System.Collections.SortedList _MetaInstructions = GetAttributes(reader);

                    if (!(_MetaInstructions == null)
                        & _MetaInstructions.ContainsKey("className")
                        & _MetaInstructions.ContainsKey("size")
                        & _MetaInstructions.ContainsKey("type"))
                    {
                        _FieldValue = CreateArray(target, System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("size"))), System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("type"))));
                    }
                    else
                    {
                        _FieldValue = CreateClass(_MetaInstructions, target, _Field);
                    }
                    //END: FIX: July 13, 2006

                    if (_FieldValue is System.Enum)
                    {
                        MoveToValueNode(reader);

                        //Translate String Value to Enumerator Value and Assign It to the Property
                        object _EnumValue = System.Enum.Parse(_FieldValue.GetType(), reader.Value);
                        _Field.SetValue(target, _EnumValue);

                        MoveToNextTag(reader);
                    }
                    else if (IsDataType(_FieldValue) | IsDataType(_Field))
                    {
                        MoveToValueNode(reader);
                        SaveValue(ref target, _Field, reader.Value);
                        MoveToNextTag(reader);
                    }
                    else if (_FieldValue is System.Array)
                    {
                        //Feature: Added Support for System.Array.
                        System.Array _Arr = (System.Array)ReadXml(reader, _FieldValue);
                        _Field.SetValue(target, _Arr);
                    }
                    else if (_FieldValue is System.Collections.IEnumerable)
                    {
                        target = ReadChildren(reader, target, _FieldValue);
                    }
                    else
                    {
                        _FieldValue = ReadXml(reader, _FieldValue);
                    }

                    break;
                }
            }
            return target;
        }


        /// <summary>
        /// Assings the Xml values to the properties.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <param name="target">The class containing the fields to update.</param>
        /// <returns>The updated class.</returns>
        /// <remarks></remarks>
        private object ReadProperties(System.Xml.XmlReader reader, object target)
        {
            object _PropertyValue;

            foreach (System.Reflection.PropertyInfo _Property in target.GetType().GetProperties(BindingCriteria))
            {
                if (string.Compare(reader.Name, _Property.Name, true) == 0)
                {
                    //BEGIN: FIX: July 13, 2006 - Check for Array Data Type
                    System.Collections.SortedList _MetaInstructions = GetAttributes(reader);

                    if (!(_MetaInstructions == null)
                        & _MetaInstructions.ContainsKey("className")
                        & _MetaInstructions.ContainsKey("size")
                        & _MetaInstructions.ContainsKey("type"))
                    {
                        _PropertyValue = CreateArray(target, System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("size"))), System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("type"))));
                    }
                    else
                    {
                        _PropertyValue = CreateClass(_MetaInstructions, target, _Property);
                    }
                    //END: FIX: July 13, 2006

                    if (_PropertyValue is System.Enum)
                    {
                        MoveToValueNode(reader);

                        //Translate String Value to Enumerator Value and Assign It to the Property
                        object _EnumValue = System.Enum.Parse(_PropertyValue.GetType(), reader.Value);
                        _Property.SetValue(target, _EnumValue, null);

                        MoveToNextTag(reader);
                    }
                    else if (IsDataType(_PropertyValue) | IsDataType(_Property))
                    {
                        MoveToValueNode(reader);
                        SaveValue(ref target, _Property, reader.Value);
                        MoveToNextTag(reader);
                    }
                    else if (_PropertyValue is System.Array)
                    {
                        //Feature: Added Support for System.Array.
                        System.Array _Arr = (System.Array)ReadXml(reader, _PropertyValue);
                        _Property.SetValue(target, _Arr, null);
                    }
                    else if (_PropertyValue is System.Collections.IEnumerable)
                    {
                        target = ReadChildren(reader, target, _PropertyValue);
                    }
                    else
                    {
                        _PropertyValue = ReadXml(reader, _PropertyValue);
                    }

                    break;
                }
            }

            return target;
        }


        /// <summary>
        /// Analyzes the Xml to build the child objects adding them to the list property.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <param name="target">The class containing the list field to update.</param>
        /// <param name="propertyMember">The IEnumerable property.</param>
        /// <returns>The updated class.</returns>
        /// <remarks></remarks>
        private object ReadChildren(System.Xml.XmlReader reader, object target, object propertyMember)
        {
            System.Collections.SortedList _MetaInstructions;
            object _NewValue;

            //Initialize the class
            ExecuteClearMethod(propertyMember);
            MoveToNextNode(reader);

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                //Get Attributes to Identify Type of Object to Create
                _MetaInstructions = GetAttributes(reader);

                //Instantiate New Object: Identify the type of object to add to the list and create it
                //BEGIN: FIX: July 13, 2006 - Check for Array Data Type
                if (!(_MetaInstructions == null)
                    & _MetaInstructions.ContainsKey("className")
                    & _MetaInstructions.ContainsKey("size")
                    & _MetaInstructions.ContainsKey("type"))
                    _NewValue = CreateArray(target, System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("size"))), System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("type"))));
                else if (!(_MetaInstructions == null) & (_MetaInstructions.ContainsKey("className")))
                {
                    _NewValue = InstantiateMember(target.GetType().Assembly, System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("className"))));
                    //END: FIX: July 13, 2006
                }
                else if (!(_MetaInstructions == null) & (_MetaInstructions.ContainsKey("className")))
                {
                    //FIX: December 12, 2006 - Support for Queue
                    _NewValue = InstantiateMember(target.GetType().Assembly, System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("className"))));
                }
                else
                {
                    //In the event the attributes don't record the className attempt to find the item 
                    //property which will tell us what type to instantiate.
                    //With this approach we must assume that all items in the list are of the same type.
                    _NewValue = null;

                    //Search Members for Item Property
                    foreach (System.Reflection.PropertyInfo _ItemMember in propertyMember.GetType().GetProperties())
                    {
                        if (string.Compare(_ItemMember.Name, "Item", true) == 0)
                        {
                            _NewValue = InstantiateMember(propertyMember.GetType().Assembly, _ItemMember.PropertyType.FullName);
                            break;
                        }
                    }
                }

                //Populate the Object
                _NewValue = ReadXml(reader, _NewValue);

                //Add the Object to the List
                //FIX: June 22, 2006 - Restrict value assignment to values set to something (instead of null).
                bool _Added = false;
                object _KeyValue;

                if (_MetaInstructions.IndexOfKey("key") >= 0)
                    _KeyValue = _MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("key"));
                else
                    _KeyValue = null;

                if (_NewValue != null & !(_Added))
                    _Added = ExecuteAddMethod(propertyMember, _KeyValue, _NewValue);

                if (_NewValue != null & !(_Added))
                    _Added = ExecuteEnqueueMethod(propertyMember, _KeyValue, _NewValue);

                if (_NewValue != null & !(_Added))
                    _Added = ExecutePushMethod(propertyMember, _KeyValue, _NewValue);

                if (_NewValue != null & !(_Added) & !(m_IgnoreWarnings))
                    throw new System.NotSupportedException("The class, " + target.GetType().Name + ", does not support deserialization. Missing the Add, Enqueue, or Push method.");

                MoveToNextTag(reader);
            }

            return target;
        }
        #endregion

        //Feature: Added Support for System.Array
        #region "Private Methods: Deserialization Helpers: System.Array"
        public bool IsNumeric(string s)
        {
            try
            {
                System.Int32.Parse(s);
            }
            catch
            {
                return false;
            }

            return true;
        }


        private object ReadArray(System.Xml.XmlReader reader, object target)
        {
            System.Collections.SortedList _MetaInstructions = GetAttributes(reader);
            string _ArraySize;
            string _ArrayType;
            string _ArrayPoint;
            object _NewValue;

            if (_MetaInstructions.ContainsKey("size"))
            {
                _ArraySize = System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("size")));
            }
            else
            {
                //Attempt to size array assuming that array is a Fixed Size Array
                _ArraySize = GetArraySize((System.Array)target);
            }

            if (_MetaInstructions.ContainsKey("type"))
            {
                _ArrayType = System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("type")));
            }
            else
            {
                _ArrayType = GetArrayType((System.Array)target);
            }

            target = CreateArray(target, _ArraySize, _ArrayType);
            MoveToNextNode(reader);

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                _MetaInstructions = GetAttributes(reader);

                if (_MetaInstructions.ContainsKey("point"))
                {
                    _ArrayPoint = System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("point")));
                }
                else
                {
                    _ArrayPoint = null;
                }

                MoveToNextNode(reader);
                _MetaInstructions = GetAttributes(reader);

                //Instantiate New Object: Identify the type of object to add to the list and create it
                if (_MetaInstructions.ContainsKey("className"))
                {
                    _NewValue = InstantiateMember(target.GetType().Assembly, System.Convert.ToString(_MetaInstructions.GetByIndex(_MetaInstructions.IndexOfKey("className"))));
                }
                else if (IsDataType(reader))
                {
                    //Do Nothing
                    _NewValue = null;
                }
                else
                {
                    _NewValue = InstantiateMember(target.GetType().Assembly, _ArrayType);
                }

                //Get Element Value
                _NewValue = ReadXml(reader, _NewValue);

                //Move Past Value
                MoveToNextTag(reader);

                //Add Element to Array
                ((System.Array)target).SetValue(_NewValue, ReadIndexes(_ArrayPoint));

                //Move Past Closing Array Item Tag
                MoveToNextTag(reader);
            }

            return target;
        }


        private System.Array CreateArray(object target, string arraySize, string arrayType)
        {
            //We have to increase the array size by a factor of 1 to account for zero-indexing.
            int[] _Indexes = ReadIndexes(arraySize);

            System.Type _ArrayType;
            switch (arrayType.ToUpper().Trim())
            {
                case "SYSTEM.OBJECT":
                    _ArrayType = System.Type.GetType("System.Object", true, false);
                    break;
                case "OBJECT":
                    _ArrayType = System.Type.GetType("System.Object", true, false);
                    break;
                case "SYSTEM.BOOLEAN":
                    _ArrayType = System.Type.GetType("System.Boolean", true, false);
                    break;
                case "BOOLEAN":
                    _ArrayType = System.Type.GetType("System.Boolean", true, false);
                    break;
                case "BOOL":
                    _ArrayType = System.Type.GetType("System.Boolean", true, false);
                    break;
                case "SYSTEM.BYTE":
                    _ArrayType = System.Type.GetType("System.Byte", true, false);
                    break;
                case "BYTE":
                    _ArrayType = System.Type.GetType("System.Byte", true, false);
                    break;
                case "SYSTEM.CHAR":
                    _ArrayType = System.Type.GetType("System.Char", true, false);
                    break;
                case "CHAR":
                    _ArrayType = System.Type.GetType("System.Char", true, false);
                    break;
                case "SYSTEM.DATE":
                    _ArrayType = System.Type.GetType("System.DateTime", true, false);
                    break;
                case "DATE":
                    _ArrayType = System.Type.GetType("System.DateTime", true, false);
                    break;
                case "SYSTEM.DATETIME":
                    _ArrayType = System.Type.GetType("System.DateTime", true, false);
                    break;
                case "DATETIME":
                    _ArrayType = System.Type.GetType("System.DateTime", true, false);
                    break;
                case "SYSTEM.DECIMAL":
                    _ArrayType = System.Type.GetType("System.Decimal", true, false);
                    break;
                case "DECIMAL":
                    _ArrayType = System.Type.GetType("System.Decimal", true, false);
                    break;
                case "SYSTEM.DOUBLE":
                    _ArrayType = System.Type.GetType("System.Double", true, false);
                    break;
                case "DOUBLE":
                    _ArrayType = System.Type.GetType("System.Double", true, false);
                    break;
                case "SYSTEM.INT16":
                    _ArrayType = System.Type.GetType("System.Int16", true, false);
                    break;
                case "INT16":
                    _ArrayType = System.Type.GetType("System.Int16", true, false);
                    break;
                case "SYSTEM.INT32":
                    _ArrayType = System.Type.GetType("System.Int32", true, false);
                    break;
                case "INT32":
                    _ArrayType = System.Type.GetType("System.Int32", true, false);
                    break;
                case "SYSTEM.INT64":
                    _ArrayType = System.Type.GetType("System.Int64", true, false);
                    break;
                case "INT64":
                    _ArrayType = System.Type.GetType("System.Int64", true, false);
                    break;
                case "SYSTEM.INTEGER":
                    _ArrayType = System.Type.GetType("System.Int32", true, false);
                    break;
                case "INTEGER":
                    _ArrayType = System.Type.GetType("System.Int32", true, false);
                    break;
                case "INT":
                    _ArrayType = System.Type.GetType("System.Int32", true, false);
                    break;
                case "SYSTEM.LONG":
                    _ArrayType = System.Type.GetType("System.Int64", true, false);
                    break;
                case "LONG":
                    _ArrayType = System.Type.GetType("System.Int64", true, false);
                    break;
                case "SYSTEM.SBYTE":
                    _ArrayType = System.Type.GetType("System.SByte", true, false);
                    break;
                case "SBYTE":
                    _ArrayType = System.Type.GetType("System.SByte", true, false);
                    break;
                case "SYSTEM.SHORT":
                    _ArrayType = System.Type.GetType("System.Int16", true, false);
                    break;
                case "SHORT":
                    _ArrayType = System.Type.GetType("System.Int16", true, false);
                    break;
                case "SYSTEM.SINGLE":
                    _ArrayType = System.Type.GetType("System.Single", true, false);
                    break;
                case "SINGLE":
                    _ArrayType = System.Type.GetType("System.Single", true, false);
                    break;
                case "FLOAT":
                    _ArrayType = System.Type.GetType("System.Single", true, false);
                    break;
                case "SYSTEM.STRING":
                    _ArrayType = System.Type.GetType("System.String", true, false);
                    break;
                case "STRING":
                    _ArrayType = System.Type.GetType("System.String", true, false);
                    break;
                case "SYSTEM.UINT16":
                    _ArrayType = System.Type.GetType("System.UInt16", true, false);
                    break;
                case "UINT16":
                    _ArrayType = System.Type.GetType("System.UInt16", true, false);
                    break;
                case "SYSTEM.UINT32":
                    _ArrayType = System.Type.GetType("System.UInt32", true, false);
                    break;
                case "UINT32":
                    _ArrayType = System.Type.GetType("System.UInt32", true, false);
                    break;
                case "SYSTEM.UINT64":
                    _ArrayType = System.Type.GetType("System.UInt64", true, false);
                    break;
                case "UINT64":
                    _ArrayType = System.Type.GetType("System.UInt64", true, false);
                    break;
                case "SYSTEM.UINTPTR":
                    _ArrayType = System.Type.GetType("System.UIntPtr", true, false);
                    break;
                case "UINTPTR":
                    _ArrayType = System.Type.GetType("System.UIntPtr", true, false);
                    break;
                case "SYSTEM.INTPTR":
                    _ArrayType = System.Type.GetType("System.IntPtr", true, false);
                    break;
                case "INTPTR":
                    _ArrayType = System.Type.GetType("System.IntPtr", true, false);
                    break;
                case "SYSTEM.UINTEGER":
                    _ArrayType = System.Type.GetType("System.UInt32", true, false);
                    break;
                case "UINTEGER":
                    _ArrayType = System.Type.GetType("System.UInt32", true, false);
                    break;
                case "UINT":
                    _ArrayType = System.Type.GetType("System.UInt32", true, false);
                    break;
                case "SYSTEM.ULONG":
                    _ArrayType = System.Type.GetType("System.UInt64", true, false);
                    break;
                case "ULONG":
                    _ArrayType = System.Type.GetType("System.UInt64", true, false);
                    break;
                case "SYSTEM.USHORT":
                    _ArrayType = System.Type.GetType("System.UInt16", true, false);
                    break;
                case "USHORT":
                    _ArrayType = System.Type.GetType("System.UInt16", true, false);
                    break;
                default:
                    try
                    {
                        _ArrayType = target.GetType().Module.GetType(arrayType, true);
                    }
                    catch (System.Exception ex)
                    {
                        throw new System.InvalidCastException("An error occurred while creating an array. Casting to type, " + arrayType + ", is not supported.", ex);
                    }
                    break;
            }

            return System.Array.CreateInstance(_ArrayType, _Indexes);
        }


        private int[] ReadIndexes(string indexes)
        {
            string[] _Sizes = indexes.Split(System.Convert.ToChar(","));
            //C# Is 0 Based While VB Is 1 Based By Default
            int[] _Lengths = new int[_Sizes.Length];
            int _Index = 0;

            foreach (string _Size in _Sizes)
            {
                if (IsNumeric(_Size))
                {
                    _Lengths[_Index] = System.Convert.ToInt32(_Size);
                }

                _Index += 1;
            }

            return _Lengths;
        }
        #endregion

        #region "Private Methods: Serialization Helpers"
        private void WriteKey(ref object source, System.Xml.XmlWriter writer)
        {
            if (source is System.Collections.DictionaryEntry)
            {
                //Record the key value so we can re-assign the key/value pair when deserializing
                writer.WriteAttributeString("key", null, ((System.Collections.DictionaryEntry)source).Key.ToString());

                //Advance to the object to serialize
                source = ((System.Collections.DictionaryEntry)source).Value;
            }
        }


        private void WriteClassName(object sourceClass, System.Xml.XmlWriter writer)
        {
            System.Type sourceType = sourceClass.GetType();

            //Record the class name so we can instantiate the object when deserializing
            //TODO: Change VB Version to match this
            if (IncludeClassNameAttribute)
            {
                writer.WriteAttributeString("className", null, sourceType.FullName.Trim());
            }
        }


        private void WriteClassName(System.Type sourceType, System.Xml.XmlWriter writer)
        {
            //Record the class name so we can instantiate the object when deserializing
            writer.WriteAttributeString("className", null, sourceType.FullName.Trim());
        }


        private void WriteString(object dataValue, System.Xml.XmlWriter writer)
        {
            if (CDataStorage & (dataValue is System.Char | dataValue is System.String | dataValue is System.Enum))
            {
                writer.WriteCData(dataValue.ToString().Trim());
            }
            else
            {
                writer.WriteString(dataValue.ToString().Trim());
            }
        }


        /// <summary>
        /// Translates the fields of object, target, into Xml Elements.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="writer">The destination for the xml.</param>
        /// <remarks></remarks>
        private void WriteFields(object source, System.Xml.XmlWriter writer)
        {
            System.Type sourceType = source.GetType();

            foreach (System.Reflection.FieldInfo _Field in sourceType.GetFields(BindingCriteria))
            {
                //FEATURE: December 9, 2006 - Check for XmlIgnore Provided by Jason Vetter
                if (_Field.GetCustomAttributes(typeof(System.Xml.Serialization.XmlIgnoreAttribute), true).Length == 0)
                {
                    object _Value = _Field.GetValue(source);

                    if (IsDataType(_Value) | _Value is System.Enum)
                    {
                        writer.WriteStartElement(_Field.Name);
                        WriteString(_Value, writer);
                        writer.WriteEndElement();
                    }
                    else if (_Value is System.Array)
                    {
                        //Feature: Added Support for System.Array
                        WriteXml(_Value, writer, _Field.Name);
                    }
                    else if (_Value is System.Collections.IEnumerable)
                    {
                        writer.WriteStartElement(_Field.Name);

                        //Record the Class Name so we can instantiate Abstract Classes when deserializing.
                        //An Abstract class is a class that inherits from a base class like System.Collections.DictionaryBase.
                        WriteClassName(_Field.FieldType, writer);

                        foreach (object _Item in ((System.Collections.IEnumerable)_Value))
                        {
                            WriteXml(_Item, writer);
                        }

                        writer.WriteEndElement();
                    }
                    else
                    {
                        WriteXml(_Value, writer, _Field.Name);
                    }
                }
            }
        }


        /// <summary>
        /// Translates the properties of object, target, into Xml Elements.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="writer">The destination for the xml.</param>
        /// <remarks></remarks>
        private void WriteProperties(object source, System.Xml.XmlWriter writer)
        {
            System.Type sourceType = source.GetType();

            foreach (System.Reflection.PropertyInfo _Property in sourceType.GetProperties(BindingCriteria))
            {
                //FEATURE: December 9, 2006 - Check for XmlIgnore Provided by Jason Vetter
                if (_Property.GetCustomAttributes(typeof(System.Xml.Serialization.XmlIgnoreAttribute), true).Length == 0)
                {
                    object _Value;

                    try
                    {
                        _Value = _Property.GetValue(source, null);
                    }
                    catch (System.Reflection.TargetParameterCountException ex1)
                    {
                        //This error may happen on a property like Item(ByVal Index As Integer)
                        //because the deserializer doesn't support properties with parameters.
                        if (IgnoreWarnings)
                        {
                            _Value = null;
                        }
                        //FIX: July 13, 2006 - Don't report valid error scenarios
                        else if (string.Compare(_Property.Name, "Item", true) == 0)
                        {
                            _Value = null;
                        }
                        else
                        {
                            //FEATURE: July 13, 2006 - Make error message more meaningful
                            throw new System.NotSupportedException("The property, " + _Property.Name + ", expects parameters. Property parameters are not supported.", ex1);
                        }
                    }
                    catch (System.Exception ex2)
                    {
                        throw ex2;
                    }

                    if (IsDataType(_Value) | _Value is System.Enum)
                    {
                        writer.WriteStartElement(_Property.Name);
                        WriteString(_Value, writer);
                        writer.WriteEndElement();
                    }
                    else if (_Value is System.Array)
                    {
                        //Feature: Added Support for System.Array
                        WriteXml(_Value, writer, _Property.Name);
                    }
                    else if (_Value is System.Collections.IEnumerable)
                    {
                        writer.WriteStartElement(_Property.Name);

                        //Record the Class Name so we can instantiate Abstract Classes when deserializing.
                        //An Abstract class is a class that inherits from a base class like System.Collections.DictionaryBase.
                        WriteClassName(_Property.PropertyType, writer);

                        foreach (object _Item in ((System.Collections.IEnumerable)_Value))
                        {
                            WriteXml(_Item, writer);
                        }

                        writer.WriteEndElement();
                    }
                    else
                    {
                        WriteXml(_Value, writer, _Property.Name);
                    }
                }
            }
        }
        #endregion

        //Feature: Added Support for System.Array
        #region "Private Methods: Serialization Helpers: System.Array"
        //Identifies the size of each dimension in the array
        private string GetArraySize(System.Array arr)
        {
            string _Size = System.String.Empty;

            for (int _Index = 0; _Index < arr.Rank; _Index++)
            {
                _Size += "," + System.Convert.ToString(arr.GetUpperBound(_Index) - arr.GetLowerBound(_Index) + 1);
            }

            if (_Size.Length == 0)
            {
                _Size = ",";
            }

            return _Size.Substring(1);
        }


        private string GetArrayType(System.Array arr)
        {
            return arr.GetType().FullName.Substring(0, arr.GetType().FullName.IndexOf("["));
        }


        //Translate current point into Xml friendly text
        private string GetArrayPoint(int[] indices)
        {
            string _Indexes = System.String.Empty;

            foreach (int _Index in indices)
            {
                _Indexes += "," + System.Convert.ToString(_Index);
            }

            if (_Indexes.Length == 0)
            {
                _Indexes = ",";
            }

            return _Indexes.Substring(1);
        }


        private void WriteArray(System.Array arr, System.Xml.XmlWriter writer)
        {
            //C# Is 0 Based While VB Is 1 Based By Default
            WriteArray(arr, writer, 0, null);
        }


        private void WriteArray(System.Array arr, System.Xml.XmlWriter writer, int rank, int[] Indices)
        {
            int _Index;

            //C# Is 0 Based While VB Is 1 Based By Default
            int[] _Indexes = new int[arr.Rank];

            if (Indices != null)
            {
                System.Array.Copy(Indices, _Indexes, Indices.Length - 1);
            }

            for (_Index = arr.GetLowerBound(rank); _Index <= arr.GetUpperBound(rank); _Index++)
            {
                //C# Is 0 Based While VB Is 1 Based By Default
                _Indexes.SetValue(_Index, rank);

                if (arr.GetValue(_Indexes) != null)
                {
                    writer.WriteStartElement("System.Array.Item");
                    writer.WriteAttributeString("point", null, GetArrayPoint(_Indexes));
                    WriteXml(arr.GetValue(_Indexes), writer);
                    writer.WriteEndElement();
                }

                if (arr.Rank - 1 > rank)
                {
                    WriteArray(arr, writer, rank + 1, _Indexes);
                }
            }
        }
        #endregion

        #region "Private Methods: Reader Helpers"
        /// <summary>
        /// Advances to the first node
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <remarks></remarks>
        private void MoveToRootNode(System.Xml.XmlReader reader)
        {
            while (reader.NodeType == System.Xml.XmlNodeType.None &
                reader.NodeType != System.Xml.XmlNodeType.Comment &
                reader.NodeType != System.Xml.XmlNodeType.Notation &
                reader.NodeType != System.Xml.XmlNodeType.ProcessingInstruction &
                reader.NodeType != System.Xml.XmlNodeType.SignificantWhitespace &
                reader.NodeType != System.Xml.XmlNodeType.Whitespace &
                reader.NodeType != System.Xml.XmlNodeType.XmlDeclaration)
            {
                reader.Read();
            }
        }


        /// <summary>
        /// Advance to the inner node, the tag between the open and closing element tags to access the element's value.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <remarks></remarks>
        private void MoveToValueNode(System.Xml.XmlReader reader)
        {
            //The name will contain an empty string when we have landed on a value tag.
            do { reader.Read(); }
            while (!(reader.EOF) &
                (reader.Name.Trim().Length > 0 |
                reader.NodeType == System.Xml.XmlNodeType.Comment |
                reader.NodeType == System.Xml.XmlNodeType.Notation |
                reader.NodeType == System.Xml.XmlNodeType.ProcessingInstruction |
                reader.NodeType == System.Xml.XmlNodeType.SignificantWhitespace |
                reader.NodeType == System.Xml.XmlNodeType.Whitespace |
                reader.NodeType == System.Xml.XmlNodeType.XmlDeclaration));
        }


        /// <summary>
        /// Advance to the next xml element.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <remarks></remarks>
        private void MoveToNextNode(System.Xml.XmlReader reader)
        {
            do { reader.Read(); }
            while (!reader.EOF &
                (reader.NodeType != System.Xml.XmlNodeType.Element));
        }


        /// <summary>
        /// Advance to the next xml tag.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <remarks></remarks>
        private void MoveToNextTag(System.Xml.XmlReader reader)
        {
            do { reader.Read(); }
            while (!(reader.EOF) &
                (reader.NodeType == System.Xml.XmlNodeType.Comment |
                reader.NodeType == System.Xml.XmlNodeType.Notation |
                reader.NodeType == System.Xml.XmlNodeType.ProcessingInstruction |
                reader.NodeType == System.Xml.XmlNodeType.SignificantWhitespace |
                reader.NodeType == System.Xml.XmlNodeType.Whitespace |
                reader.NodeType == System.Xml.XmlNodeType.XmlDeclaration));
        }


        /// <summary>
        /// Returns a key/value pair representing the attributes in the element.
        /// </summary>
        /// <param name="reader">The bufferred xml to analyze.</param>
        /// <returns>A sorted list of the attributes with the name as the key and the value as the value.</returns>
        /// <remarks>Executing this method querries the current node for the attributes without advancing to the next node.</remarks>
        private System.Collections.SortedList GetAttributes(System.Xml.XmlReader reader)
        {
            System.Collections.SortedList _List = new System.Collections.SortedList();

            //FIX: December 9, 2005 - Fix to handle reading attributes from ReadXml routine
            if ((reader.NodeType != System.Xml.XmlNodeType.None) & (reader.MoveToFirstAttribute()))
            {
                do
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Attribute)
                    {
                        _List.Add(reader.Name, reader.Value);
                    }
                }
                while (reader.MoveToNextAttribute() | reader.NodeType != System.Xml.XmlNodeType.Attribute);
            }

            if (reader.NodeType == System.Xml.XmlNodeType.Attribute)
            {
                reader.MoveToElement();
            }

            return _List;
        }
        #endregion

        #region "Private Methods:System.Reflection Helpers"
        /// <summary>
        /// Identifies if the object is a data type.
        /// </summary>
        /// <param name="Value">The object to test.</param>
        /// <returns>true if the value is a data type.</returns>
        /// <remarks></remarks>
        private bool IsDataType(object dataValue)
        {
            return IsDataType(dataValue, false);
        }


        /// <summary>
        /// Identifies if the object is a data type.
        /// </summary>
        /// <param name="Value">The object to test.</param>
        /// <param name="valueIsTypeName">Indicates that the dataValue is the name of the data type and not the actual data value.</param>
        /// <returns>true if the value is a data type.</returns>
        /// <remarks></remarks>
        private bool IsDataType(object dataValue, bool valueIsTypeName)
        {
            string _ValueType;

            //Retrieve the data type from the value passed in.
            if (valueIsTypeName)
            {
                //FIX: December 9, 2005 - Fix to handle reading attributes from ReadXml routine
                if (dataValue == null)
                {
                    _ValueType = string.Empty;
                }
                else
                {
                    _ValueType = dataValue.ToString();
                }
            }
            else if (dataValue is System.Xml.XmlReader)
            {
                _ValueType = ((System.Xml.XmlReader)dataValue).Name;
            }
            else if (dataValue is System.Reflection.FieldInfo)
            {
                _ValueType = ((System.Reflection.FieldInfo)dataValue).FieldType.FullName;
            }
            else if (dataValue is System.Reflection.PropertyInfo)
            {
                _ValueType = ((System.Reflection.PropertyInfo)dataValue).PropertyType.FullName;
            }
            else if (dataValue is System.Type)
            {
                System.Type dataValueType = (System.Type)dataValue;
                _ValueType = dataValueType.FullName;
            }
            else if (dataValue != null)
            {
                _ValueType = dataValue.GetType().FullName;
            }
            else
            {
                _ValueType = string.Empty;
            }

            //Test the data type
            switch (_ValueType.Trim().ToUpper())
            {
                case "SYSTEM.BOOLEAN":
                    return true;
                case "BOOLEAN":
                    return true;
                case "BOOL":
                    return true;
                case "SYSTEM.BYTE":
                    return true;
                case "BYTE":
                    return true;
                case "SYSTEM.CHAR":
                    return true;
                case "CHAR":
                    return true;
                case "SYSTEM.DATE":
                    return true;
                case "DATE":
                    return true;
                case "SYSTEM.DATETIME":
                    return true;
                case "DATETIME":
                    return true;
                case "SYSTEM.DECIMAL":
                    return true;
                case "DECIMAL":
                    return true;
                case "SYSTEM.DOUBLE":
                    return true;
                case "DOUBLE":
                    return true;
                case "SYSTEM.INT16":
                    return true;
                case "INT16":
                    return true;
                case "SYSTEM.INT32":
                    return true;
                case "INT32":
                    return true;
                case "SYSTEM.INT64":
                    return true;
                case "INT64":
                    return true;
                case "SYSTEM.INTEGER":
                    return true;
                case "INTEGER":
                    return true;
                case "INT":
                    return true;
                case "SYSTEM.LONG":
                    return true;
                case "LONG":
                    return true;
                case "SYSTEM.SBYTE":
                    return true;
                case "SBYTE":
                    return true;
                case "SYSTEM.SHORT":
                    return true;
                case "SHORT":
                    return true;
                case "SYSTEM.SINGLE":
                    return true;
                case "SINGLE":
                    return true;
                case "FLOAT":
                    return true;
                case "SYSTEM.STRING":
                    return true;
                case "STRING":
                    return true;
                case "SYSTEM.UINT16":
                    return true;
                case "UINT16":
                    return true;
                case "SYSTEM.UINT32":
                    return true;
                case "UINT32":
                    return true;
                case "SYSTEM.UINT64":
                    return true;
                case "UINT64":
                    return true;
                case "SYSTEM.UINTPTR":
                    return true;
                case "UINTPTR":
                    return true;
                case "SYSTEM.INTPTR":
                    return true;
                case "INTPTR":
                    return true;
                case "SYSTEM.UINTEGER":
                    return true;
                case "UINTEGER":
                    return true;
                case "UINT":
                    return true;
                case "SYSTEM.ULONG":
                    return true;
                case "ULONG":
                    return true;
                case "SYSTEM.USHORT":
                    return true;
                case "USHORT":
                    return true;
                default:
                    return false;
            }
        }


        /// <summary>
        /// Instantiates a new class.
        /// </summary>
        /// <param name="attributes">The attributes from the Xml used to identify the class type.</param>
        /// <param name="target">The class containing the reference to the member to create.</param>
        /// <param name="member">Information about the member to create.</param>
        /// <returns>The instantiated class.</returns>
        /// <remarks></remarks>
        private object CreateClass(System.Collections.SortedList attributes, object target, System.Reflection.MemberInfo member)
        {
            object _MemberValue;

            if (member is System.Reflection.FieldInfo)
            {
                _MemberValue = ((System.Reflection.FieldInfo)member).GetValue(target);
            }
            else if (member is System.Reflection.PropertyInfo)
            {
                _MemberValue = ((System.Reflection.PropertyInfo)member).GetValue(target, null);
            }
            else
            {
                throw new System.NotSupportedException("MemberInfo type, " + member.GetType().Name + ", not supported.");
            }

            //Instantiate Object Types
            if ((_MemberValue == null) & !(IsDataType(member)))
            {
                string _MemberType = null;

                //Create a New Instance of the Object 
                if (attributes != null & attributes.ContainsKey("className"))
                {
                    _MemberType = System.Convert.ToString(attributes.GetByIndex(attributes.IndexOfKey("className")));
                    _MemberValue = InstantiateMember(target.GetType().Assembly, _MemberType);
                }

                if (_MemberValue == null)
                {
                    //Attempt to create the class based on the field type.
                    //This won't work if the field is type System.Object.
                    if (member is System.Reflection.FieldInfo)
                    {
                        _MemberType = ((System.Reflection.FieldInfo)member).FieldType.FullName;
                    }
                    else if (member is System.Reflection.PropertyInfo)
                    {
                        _MemberType = ((System.Reflection.PropertyInfo)member).PropertyType.FullName;
                    }

                    _MemberValue = InstantiateMember(target.GetType().Assembly, _MemberType);
                }

                //Assign the new class to the target object's field
                if (member is System.Reflection.FieldInfo)
                {
                    ((System.Reflection.FieldInfo)member).SetValue(target, _MemberValue);
                }
                else if (member is System.Reflection.PropertyInfo)
                {
                    ((System.Reflection.PropertyInfo)member).SetValue(target, _MemberValue, null);
                }
            }

            return _MemberValue;
        }


        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="classAssembly">The assembly where the class to create is stored.</param>
        /// <param name="className">The fullname of the class to create.</param>
        /// <returns>If successful the instantiated class, otherwise null.</returns>
        /// <remarks></remarks>
        private object InstantiateMember(System.Reflection.Assembly classAssembly, string className)
        {
            object _Value = null;

            //Create a New Instance of the Object
            try
            {
                _Value = classAssembly.CreateInstance(className);
            }
            catch (System.MissingMethodException missingDefaultConstructorException)
            {
                //This error is generated by System.String.
                //It can also occur if the class is missing a default constructor.
                object dontWantToUseVarAndDontWantToSeeErrorMessageOnCompile = missingDefaultConstructorException;
            }
            catch (System.Exception unhandledException)
            {
                throw unhandledException;
            }

            //We have to verify that the value returned isn't a data type.
            //For example, string values require a parameter for the constructor
            //but work just fine without initialization.
            if ((_Value == null) & !(IsDataType(_Value)) & !(IsDataType(className, true)))
            {
                //The object to instantiate did not exist in the same assembly namespace.
                //Attempt to find the class and instantiate it.
                //This scenario happens with System namespace objects like System.Collections.ArrayList.
                _Value = System.Type.GetType(className).Assembly.CreateInstance(className);
            }
            if ((_Value == null) & !(IsDataType(_Value)) & !(IsDataType(className, true)))
            {
                //The member did not have a value and we were unable to instantiate a new class to assign to it.
                throw new System.MissingMethodException("Unable to deserialize Xml into " + className + ". Failed to initialize the object. Verify that the member's type supports a default constructor or that the member is automatically instantiated when the parent class is instantiated.");
            }

            return _Value;
        }


        /// <summary>
        /// Assigns a casted value to the target object.
        /// </summary>
        /// <param name="target">The object to update.</param>
        /// <param name="valueType">The type to cast the value to.</param>
        /// <param name="value">The value to cast and assign.</param>
        /// <remarks>
        /// This has to be done to support assignment to values of type object.
        /// Without this code all values would be assigned as strings.
        /// </remarks>
        private void SaveValue(ref object target, string valueType, object dataValue)
        {
            switch (valueType.ToUpper().Trim())
            {
                case "SYSTEM.BOOLEAN":
                    target = System.Convert.ToBoolean(dataValue);
                    break;
                case "BOOLEAN":
                    target = System.Convert.ToBoolean(dataValue);
                    break;
                case "BOOL":
                    target = System.Convert.ToBoolean(dataValue);
                    break;
                case "SYSTEM.BYTE":
                    target = System.Convert.ToByte(dataValue);
                    break;
                case "BYTE":
                    target = System.Convert.ToByte(dataValue);
                    break;
                case "SYSTEM.CHAR":
                    target = System.Convert.ToChar(dataValue);
                    break;
                case "CHAR":
                    target = System.Convert.ToChar(dataValue);
                    break;
                case "SYSTEM.DATE":
                    target = System.Convert.ToDateTime(dataValue);
                    break;
                case "DATE":
                    target = System.Convert.ToDateTime(dataValue);
                    break;
                case "SYSTEM.DATETIME":
                    target = System.Convert.ToDateTime(dataValue);
                    break;
                case "DATETIME":
                    target = System.Convert.ToDateTime(dataValue);
                    break;
                case "SYSTEM.DECIMAL":
                    target = System.Convert.ToDecimal(dataValue);
                    break;
                case "DECIMAL":
                    target = System.Convert.ToDecimal(dataValue);
                    break;
                case "SYSTEM.DOUBLE":
                    target = System.Convert.ToDouble(dataValue);
                    break;
                case "DOUBLE":
                    target = System.Convert.ToDouble(dataValue);
                    break;
                case "SYSTEM.INT16":
                    target = System.Convert.ToInt16(dataValue);
                    break;
                case "INT16":
                    target = System.Convert.ToInt16(dataValue);
                    break;
                case "SYSTEM.INT32":
                    target = System.Convert.ToInt32(dataValue);
                    break;
                case "INT32":
                    target = System.Convert.ToInt32(dataValue);
                    break;
                case "SYSTEM.INT64":
                    target = System.Convert.ToInt64(dataValue);
                    break;
                case "INT64":
                    target = System.Convert.ToInt64(dataValue);
                    break;
                case "SYSTEM.INTEGER":
                    target = System.Convert.ToInt32(dataValue);
                    break;
                case "INTEGER":
                    target = System.Convert.ToInt32(dataValue);
                    break;
                case "INT":
                    target = System.Convert.ToInt32(dataValue);
                    break;
                case "SYSTEM.LONG":
                    target = System.Convert.ToInt64(dataValue);
                    break;
                case "LONG":
                    target = System.Convert.ToInt64(dataValue);
                    break;
                case "SYSTEM.SBYTE":
                    target = System.Convert.ToSByte(dataValue);
                    break;
                case "SBYTE":
                    target = System.Convert.ToSByte(dataValue);
                    break;
                case "SYSTEM.SHORT":
                    target = System.Convert.ToInt16(dataValue);
                    break;
                case "SHORT":
                    target = System.Convert.ToInt16(dataValue);
                    break;
                case "SYSTEM.SINGLE":
                    target = System.Convert.ToSingle(dataValue);
                    break;
                case "SINGLE":
                    target = System.Convert.ToSingle(dataValue);
                    break;
                case "FLOAT":
                    target = System.Convert.ToSingle(dataValue);
                    break;
                case "SYSTEM.STRING":
                    target = System.Convert.ToString(dataValue);
                    break;
                case "STRING":
                    target = System.Convert.ToString(dataValue);
                    break;
                case "SYSTEM.UINT16":
                    target = System.Convert.ToUInt16(dataValue);
                    break;
                case "UINT16":
                    target = System.Convert.ToUInt16(dataValue);
                    break;
                case "SYSTEM.UINT32":
                    target = System.Convert.ToUInt32(dataValue);
                    break;
                case "UINT32":
                    target = System.Convert.ToUInt32(dataValue);
                    break;
                case "SYSTEM.UINT64":
                    target = System.Convert.ToUInt64(dataValue);
                    break;
                case "UINT64":
                    target = System.Convert.ToUInt64(dataValue);
                    break;
                case "SYSTEM.UINTPTR":
                    target = (System.UIntPtr)dataValue;
                    break;
                case "UINTPTR":
                    target = (System.UIntPtr)dataValue;
                    break;
                case "SYSTEM.INTPTR":
                    target = (System.IntPtr)dataValue;
                    break;
                case "INTPTR":
                    target = (System.IntPtr)dataValue;
                    break;
                case "SYSTEM.UINTEGER":
                    target = System.Convert.ToUInt32(dataValue);
                    break;
                case "UINTEGER":
                    target = System.Convert.ToUInt32(dataValue);
                    break;
                case "UINT":
                    target = System.Convert.ToUInt32(dataValue);
                    break;
                case "SYSTEM.ULONG":
                    target = System.Convert.ToUInt64(dataValue);
                    break;
                case "ULONG":
                    target = System.Convert.ToUInt64(dataValue);
                    break;
                case "SYSTEM.USHORT":
                    target = System.Convert.ToUInt16(dataValue);
                    break;
                case "USHORT":
                    target = System.Convert.ToUInt16(dataValue);
                    break;
                case "SYSTEM.OBJECT":
                    //FIX: July 13, 2006 - System.Object Support
                    target = dataValue;
                    break;
                case "OBJECT":
                    //FIX: July 13, 2006 - System.Object Support
                    target = dataValue;
                    break;
                default:
                    throw new System.InvalidCastException("An error occurred while assigning a value to " + target.GetType().FullName + ". Casting to type, " + valueType.ToUpper().Trim() + ", is not supported.");
            }
        }


        /// <summary>
        /// Assigns a strongly-typed data type to the member.
        /// </summary>
        /// <param name="target">The object containing the field.</param>
        /// <param name="targetField">A FieldInfo object representing the field to update.</param>
        /// <param name="value">The value to assign to the field.</param>
        /// <remarks>
        /// This has to be done to support assignment to fields of type object.
        /// Without this code all values would be assigned as strings.
        /// </remarks>
        private void SaveValue(ref object target, System.Reflection.FieldInfo targetField, object dataValue)
        {
            switch (targetField.FieldType.FullName.ToUpper())
            {
                case "SYSTEM.BOOLEAN":
                    targetField.SetValue(target, System.Convert.ToBoolean(dataValue));
                    break;
                case "BOOLEAN":
                    targetField.SetValue(target, System.Convert.ToBoolean(dataValue));
                    break;
                case "BOOL":
                    targetField.SetValue(target, System.Convert.ToBoolean(dataValue));
                    break;
                case "SYSTEM.BYTE":
                    targetField.SetValue(target, System.Convert.ToByte(dataValue));
                    break;
                case "BYTE":
                    targetField.SetValue(target, System.Convert.ToByte(dataValue));
                    break;
                case "SYSTEM.CHAR":
                    targetField.SetValue(target, System.Convert.ToChar(dataValue));
                    break;
                case "CHAR":
                    targetField.SetValue(target, System.Convert.ToChar(dataValue));
                    break;
                case "SYSTEM.DATE":
                    targetField.SetValue(target, System.Convert.ToDateTime(dataValue));
                    break;
                case "DATE":
                    targetField.SetValue(target, System.Convert.ToDateTime(dataValue));
                    break;
                case "SYSTEM.DATETIME":
                    targetField.SetValue(target, System.Convert.ToDateTime(dataValue));
                    break;
                case "DATETIME":
                    targetField.SetValue(target, System.Convert.ToDateTime(dataValue));
                    break;
                case "SYSTEM.DECIMAL":
                    targetField.SetValue(target, System.Convert.ToDecimal(dataValue));
                    break;
                case "DECIMAL":
                    targetField.SetValue(target, System.Convert.ToDecimal(dataValue));
                    break;
                case "SYSTEM.DOUBLE":
                    targetField.SetValue(target, System.Convert.ToDouble(dataValue));
                    break;
                case "DOUBLE":
                    targetField.SetValue(target, System.Convert.ToDouble(dataValue));
                    break;
                case "SYSTEM.INT16":
                    targetField.SetValue(target, System.Convert.ToInt16(dataValue));
                    break;
                case "INT16":
                    targetField.SetValue(target, System.Convert.ToInt16(dataValue));
                    break;
                case "SYSTEM.INT32":
                    targetField.SetValue(target, System.Convert.ToInt32(dataValue));
                    break;
                case "INT32":
                    targetField.SetValue(target, System.Convert.ToInt32(dataValue));
                    break;
                case "SYSTEM.INT64":
                    targetField.SetValue(target, System.Convert.ToInt64(dataValue));
                    break;
                case "INT64":
                    targetField.SetValue(target, System.Convert.ToInt64(dataValue));
                    break;
                case "SYSTEM.INTEGER":
                    targetField.SetValue(target, System.Convert.ToInt32(dataValue));
                    break;
                case "INTEGER":
                    targetField.SetValue(target, System.Convert.ToInt32(dataValue));
                    break;
                case "INT":
                    targetField.SetValue(target, System.Convert.ToInt32(dataValue));
                    break;
                case "SYSTEM.LONG":
                    targetField.SetValue(target, System.Convert.ToInt64(dataValue));
                    break;
                case "LONG":
                    targetField.SetValue(target, System.Convert.ToInt64(dataValue));
                    break;
                case "SYSTEM.SBYTE":
                    targetField.SetValue(target, System.Convert.ToSByte(dataValue));
                    break;
                case "SBYTE":
                    targetField.SetValue(target, System.Convert.ToSByte(dataValue));
                    break;
                case "SYSTEM.SHORT":
                    targetField.SetValue(target, System.Convert.ToInt16(dataValue));
                    break;
                case "SHORT":
                    targetField.SetValue(target, System.Convert.ToInt16(dataValue));
                    break;
                case "SYSTEM.SINGLE":
                    targetField.SetValue(target, System.Convert.ToSingle(dataValue));
                    break;
                case "SINGLE":
                    targetField.SetValue(target, System.Convert.ToSingle(dataValue));
                    break;
                case "FLOAT":
                    targetField.SetValue(target, System.Convert.ToSingle(dataValue));
                    break;
                case "SYSTEM.STRING":
                    targetField.SetValue(target, System.Convert.ToString(dataValue));
                    break;
                case "STRING":
                    targetField.SetValue(target, System.Convert.ToString(dataValue));
                    break;
                case "SYSTEM.UINT16":
                    targetField.SetValue(target, System.Convert.ToUInt16(dataValue));
                    break;
                case "UINT16":
                    targetField.SetValue(target, System.Convert.ToUInt16(dataValue));
                    break;
                case "SYSTEM.UINT32":
                    targetField.SetValue(target, System.Convert.ToUInt32(dataValue));
                    break;
                case "UINT32":
                    targetField.SetValue(target, System.Convert.ToUInt32(dataValue));
                    break;
                case "SYSTEM.UINT64":
                    targetField.SetValue(target, System.Convert.ToUInt64(dataValue));
                    break;
                case "UINT64":
                    targetField.SetValue(target, System.Convert.ToUInt64(dataValue));
                    break;
                case "SYSTEM.UINTPTR":
                    targetField.SetValue(target, (System.UIntPtr)dataValue);
                    break;
                case "UINTPTR":
                    targetField.SetValue(target, (System.UIntPtr)dataValue);
                    break;
                case "SYSTEM.INTPTR":
                    targetField.SetValue(target, (System.IntPtr)dataValue);
                    break;
                case "INTPTR":
                    targetField.SetValue(target, (System.IntPtr)dataValue);
                    break;
                case "SYSTEM.UINTEGER":
                    targetField.SetValue(target, System.Convert.ToUInt32(dataValue));
                    break;
                case "UINTEGER":
                    targetField.SetValue(target, System.Convert.ToUInt32(dataValue));
                    break;
                case "UINT":
                    targetField.SetValue(target, System.Convert.ToUInt32(dataValue));
                    break;
                case "SYSTEM.ULONG":
                    targetField.SetValue(target, System.Convert.ToUInt64(dataValue));
                    break;
                case "ULONG":
                    targetField.SetValue(target, System.Convert.ToUInt64(dataValue));
                    break;
                case "SYSTEM.USHORT":
                    targetField.SetValue(target, System.Convert.ToUInt16(dataValue));
                    break;
                case "USHORT":
                    targetField.SetValue(target, System.Convert.ToUInt16(dataValue));
                    break;
                case "SYSTEM.OBJECT":
                    //FIX: July 13, 2006 - System.Object Support
                    targetField.SetValue(target, dataValue);
                    break;
                case "OBJECT":
                    //FIX: July 13, 2006 - System.Object Support
                    targetField.SetValue(target, dataValue);
                    break;
                default:
                    throw new System.InvalidCastException("An error occurred while assigning a value to " + target.GetType().FullName + ". Casting to type, " + targetField.FieldType.FullName.ToUpper() + ", is not supported.");
            }
        }


        /// <summary>
        /// Assigns a strongly-typed data type to the member.
        /// </summary>
        /// <param name="target">The object containing the property.</param>
        /// <param name="targetProperty">A PropertyInfo object representing the property to update.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <remarks>
        /// This has to be done to support assignment to properties of type object.
        /// Without this code all values would be assigned as strings.
        /// </remarks>
        private void SaveValue(ref object target, System.Reflection.PropertyInfo targetProperty, object dataValue)
        {
            if (targetProperty.CanWrite)
            {
                switch (targetProperty.PropertyType.FullName.ToUpper())
                {
                    case "SYSTEM.BOOLEAN":
                        targetProperty.SetValue(target, System.Convert.ToBoolean(dataValue), null);
                        break;
                    case "BOOLEAN":
                        targetProperty.SetValue(target, System.Convert.ToBoolean(dataValue), null);
                        break;
                    case "BOOL":
                        targetProperty.SetValue(target, System.Convert.ToBoolean(dataValue), null);
                        break;
                    case "SYSTEM.BYTE":
                        targetProperty.SetValue(target, System.Convert.ToByte(dataValue), null);
                        break;
                    case "BYTE":
                        targetProperty.SetValue(target, System.Convert.ToByte(dataValue), null);
                        break;
                    case "SYSTEM.CHAR":
                        targetProperty.SetValue(target, System.Convert.ToChar(dataValue), null);
                        break;
                    case "CHAR":
                        targetProperty.SetValue(target, System.Convert.ToChar(dataValue), null);
                        break;
                    case "SYSTEM.DATE":
                        targetProperty.SetValue(target, System.Convert.ToDateTime(dataValue), null);
                        break;
                    case "DATE":
                        targetProperty.SetValue(target, System.Convert.ToDateTime(dataValue), null);
                        break;
                    case "SYSTEM.DATETIME":
                        targetProperty.SetValue(target, System.Convert.ToDateTime(dataValue), null);
                        break;
                    case "DATETIME":
                        targetProperty.SetValue(target, System.Convert.ToDateTime(dataValue), null);
                        break;
                    case "SYSTEM.DECIMAL":
                        targetProperty.SetValue(target, System.Convert.ToDecimal(dataValue), null);
                        break;
                    case "DECIMAL":
                        targetProperty.SetValue(target, System.Convert.ToDecimal(dataValue), null);
                        break;
                    case "SYSTEM.DOUBLE":
                        targetProperty.SetValue(target, System.Convert.ToDouble(dataValue), null);
                        break;
                    case "DOUBLE":
                        targetProperty.SetValue(target, System.Convert.ToDouble(dataValue), null);
                        break;
                    case "SYSTEM.INT16":
                        targetProperty.SetValue(target, System.Convert.ToInt16(dataValue), null);
                        break;
                    case "INT16":
                        targetProperty.SetValue(target, System.Convert.ToInt16(dataValue), null);
                        break;
                    case "SYSTEM.INT32":
                        targetProperty.SetValue(target, System.Convert.ToInt32(dataValue), null);
                        break;
                    case "INT32":
                        targetProperty.SetValue(target, System.Convert.ToInt32(dataValue), null);
                        break;
                    case "SYSTEM.INT64":
                        targetProperty.SetValue(target, System.Convert.ToInt64(dataValue), null);
                        break;
                    case "INT64":
                        targetProperty.SetValue(target, System.Convert.ToInt64(dataValue), null);
                        break;
                    case "SYSTEM.INTEGER":
                        targetProperty.SetValue(target, System.Convert.ToInt32(dataValue), null);
                        break;
                    case "INTEGER":
                        targetProperty.SetValue(target, System.Convert.ToInt32(dataValue), null);
                        break;
                    case "INT":
                        targetProperty.SetValue(target, System.Convert.ToInt32(dataValue), null);
                        break;
                    case "SYSTEM.LONG":
                        targetProperty.SetValue(target, System.Convert.ToInt64(dataValue), null);
                        break;
                    case "LONG":
                        targetProperty.SetValue(target, System.Convert.ToInt64(dataValue), null);
                        break;
                    case "SYSTEM.SBYTE":
                        targetProperty.SetValue(target, System.Convert.ToSByte(dataValue), null);
                        break;
                    case "SBYTE":
                        targetProperty.SetValue(target, System.Convert.ToSByte(dataValue), null);
                        break;
                    case "SYSTEM.SHORT":
                        targetProperty.SetValue(target, System.Convert.ToInt16(dataValue), null);
                        break;
                    case "SHORT":
                        targetProperty.SetValue(target, System.Convert.ToInt16(dataValue), null);
                        break;
                    case "SYSTEM.SINGLE":
                        targetProperty.SetValue(target, System.Convert.ToSingle(dataValue), null);
                        break;
                    case "SINGLE":
                        targetProperty.SetValue(target, System.Convert.ToSingle(dataValue), null);
                        break;
                    case "FLOAT":
                        targetProperty.SetValue(target, System.Convert.ToSingle(dataValue), null);
                        break;
                    case "SYSTEM.STRING":
                        targetProperty.SetValue(target, System.Convert.ToString(dataValue), null);
                        break;
                    case "STRING":
                        targetProperty.SetValue(target, System.Convert.ToString(dataValue), null);
                        break;
                    case "SYSTEM.UINT16":
                        targetProperty.SetValue(target, System.Convert.ToUInt16(dataValue), null);
                        break;
                    case "UINT16":
                        targetProperty.SetValue(target, System.Convert.ToUInt16(dataValue), null);
                        break;
                    case "SYSTEM.UINT32":
                        targetProperty.SetValue(target, System.Convert.ToUInt32(dataValue), null);
                        break;
                    case "UINT32":
                        targetProperty.SetValue(target, System.Convert.ToUInt32(dataValue), null);
                        break;
                    case "SYSTEM.UINT64":
                        targetProperty.SetValue(target, System.Convert.ToUInt64(dataValue), null);
                        break;
                    case "UINT64":
                        targetProperty.SetValue(target, System.Convert.ToUInt64(dataValue), null);
                        break;
                    case "SYSTEM.UINTPTR":
                        targetProperty.SetValue(target, (System.UIntPtr)dataValue, null);
                        break;
                    case "UINTPTR":
                        targetProperty.SetValue(target, (System.UIntPtr)dataValue, null);
                        break;
                    case "SYSTEM.INTPTR":
                        targetProperty.SetValue(target, (System.IntPtr)dataValue, null);
                        break;
                    case "INTPTR":
                        targetProperty.SetValue(target, (System.IntPtr)dataValue, null);
                        break;
                    case "SYSTEM.UINTEGER":
                        targetProperty.SetValue(target, System.Convert.ToUInt32(dataValue), null);
                        break;
                    case "UINTEGER":
                        targetProperty.SetValue(target, System.Convert.ToUInt32(dataValue), null);
                        break;
                    case "UINT":
                        targetProperty.SetValue(target, System.Convert.ToUInt32(dataValue), null);
                        break;
                    case "SYSTEM.ULONG":
                        targetProperty.SetValue(target, System.Convert.ToUInt64(dataValue), null);
                        break;
                    case "ULONG":
                        targetProperty.SetValue(target, System.Convert.ToUInt64(dataValue), null);
                        break;
                    case "SYSTEM.USHORT":
                        targetProperty.SetValue(target, System.Convert.ToUInt16(dataValue), null);
                        break;
                    case "USHORT":
                        targetProperty.SetValue(target, System.Convert.ToUInt16(dataValue), null);
                        break;
                    case "SYSTEM.OBJECT":
                        //FIX: July 13, 2006 - System.Object Support
                        targetProperty.SetValue(target, dataValue, null);
                        break;
                    case "OBJECT":
                        //FIX: July 13, 2006 - System.Object Support
                        targetProperty.SetValue(target, dataValue, null);
                        break;
                    default:
                        throw new System.InvalidCastException("An error occurred while assigning a value to " + target.GetType().FullName + ". Casting to type, " + targetProperty.PropertyType.FullName.ToUpper() + ", is not supported.");
                }
            }
            else
            {
                //Although we serialized the value we can not deserialize it because the property is marked ReadOnly.
            }
        }


        /// <summary>
        /// Executes the add method of the list object when supported.
        /// </summary>
        /// <param name="target">The class containing the method to execute.</param>
        /// <param name="Key">The key value for DictionaryEntries. Use null if a type other than DictionaryEntry.</param>
        /// <param name="value">The value to assign to the class.</param>
        /// <remarks></remarks>
        private bool ExecuteAddMethod(object target, object key, object dataValue)
        {
            bool _FoundMethod = false;

            foreach (System.Reflection.MemberInfo _ItemMethod in target.GetType().GetMethods())
            {
                if (string.Compare(_ItemMethod.Name, "Add", true) == 0)
                {
                    if (key == null)
                    {
                        target.GetType().InvokeMember("Add", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { dataValue });
                    }
                    else
                    {
                        target.GetType().InvokeMember("Add", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { key, dataValue });
                    }

                    _FoundMethod = true;
                    break;
                }
            }

            return _FoundMethod;
        }


        /// <summary>
        /// Executes the Enqueue method of the list object when supported.
        /// </summary>
        /// <param name="target">The class containing the method to execute.</param>
        /// <param name="Key">The key value for DictionaryEntries. Use null if a type other than DictionaryEntry.</param>
        /// <param name="value">The value to assign to the class.</param>
        /// <remarks></remarks>
        private bool ExecuteEnqueueMethod(object target, object key, object dataValue)
        {
            bool _FoundMethod = false;

            foreach (System.Reflection.MemberInfo _ItemMethod in target.GetType().GetMethods())
            {
                if (string.Compare(_ItemMethod.Name, "Enqueue", true) == 0)
                {
                    if (key == null)
                    {
                        target.GetType().InvokeMember("Enqueue", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { dataValue });
                    }
                    else
                    {
                        target.GetType().InvokeMember("Enqueue", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { key, dataValue });
                    }

                    _FoundMethod = true;
                    break;
                }
            }

            return _FoundMethod;
        }


        /// <summary>
        /// Executes the Push method of the list object when supported.
        /// </summary>
        /// <param name="target">The class containing the method to execute.</param>
        /// <param name="Key">The key value for DictionaryEntries. Use null if a type other than DictionaryEntry.</param>
        /// <param name="value">The value to assign to the class.</param>
        /// <remarks></remarks>
        private bool ExecutePushMethod(object target, object key, object dataValue)
        {
            bool _FoundMethod = false;

            foreach (System.Reflection.MemberInfo _ItemMethod in target.GetType().GetMethods())
            {
                if (string.Compare(_ItemMethod.Name, "Push", true) == 0)
                {
                    if (key == null)
                    {
                        target.GetType().InvokeMember("Push", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { dataValue });
                    }
                    else
                    {
                        target.GetType().InvokeMember("Push", System.Reflection.BindingFlags.InvokeMethod, null, target, new object[] { key, dataValue });
                    }

                    _FoundMethod = true;
                    break;
                }
            }

            return _FoundMethod;
        }


        /// <summary>
        /// Executes the clear method of the list object when supported.
        /// </summary>
        /// <param name="target">The class containing the method to execute.</param>
        /// <remarks></remarks>
        private void ExecuteClearMethod(object target)
        {
            foreach (System.Reflection.MemberInfo _ItemMethod in target.GetType().GetMethods())
            {
                if (string.Compare(_ItemMethod.Name, "Clear", true) == 0)
                {
                    target.GetType().InvokeMember("Clear", System.Reflection.BindingFlags.InvokeMethod, null, target, null);
                    //FIX: July 13, 2006 - Efficiency Change
                    break;
                }
            }
        }
        #endregion
    }
}