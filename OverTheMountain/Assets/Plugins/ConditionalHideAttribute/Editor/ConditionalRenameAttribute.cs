using UnityEditor;
using UnityEngine;

//Derived from ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: - 
//Arthur Uzulin - 18/10/2018

[CustomPropertyDrawer(typeof(ConditionalRenameAttribute))]
public class ConditionalRenamePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        ConditionalRenameAttribute condRAtt = (ConditionalRenameAttribute)attribute;
        bool enabled = GetConditionalRenameAttributeResult(condRAtt, property);

        EditorGUI.PropertyField(position, property, enabled ? new GUIContent(condRAtt.Name, label.image, label.tooltip) : label, true);
    }

    private bool GetConditionalRenameAttributeResult(ConditionalRenameAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = (condHAtt.UseOrLogic) ? false : true;

        //Handle primary property
        SerializedProperty sourcePropertyValue = null;
        //Get the full relative property path of the sourcefield so we can have nested hiding.Use old method when dealing with arrays
        if (!property.isArray)
        {
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
            sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            //if the find failed->fall back to the old system
            if (sourcePropertyValue == null)
            {
                //original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);
            }
        }
        else
        {
            //original implementation (doens't work with nested serializedObjects)
            sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField);
        }


        if (sourcePropertyValue != null)
        {
            enabled = CheckPropertyType(sourcePropertyValue);
            if (condHAtt.InverseCondition1) enabled = !enabled;
        }
        else
        {
            //Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }


        //handle secondary property
        SerializedProperty sourcePropertyValue2 = null;
        if (!property.isArray)
        {
            string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField2); //changes the path to the conditionalsource property path
            sourcePropertyValue2 = property.serializedObject.FindProperty(conditionPath);

            //if the find failed->fall back to the old system
            if (sourcePropertyValue2 == null)
            {
                //original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue2 = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField2);
            }
        }
        else
        {
            // original implementation(doens't work with nested serializedObjects) 
            sourcePropertyValue2 = property.serializedObject.FindProperty(condHAtt.ConditionalSourceField2);
        }

        //Combine the results
        if (sourcePropertyValue2 != null)
        {
            bool prop2Enabled = CheckPropertyType(sourcePropertyValue2);
            if (condHAtt.InverseCondition2) prop2Enabled = !prop2Enabled;

            if (condHAtt.UseOrLogic)
                enabled = enabled || prop2Enabled;
            else
                enabled = enabled && prop2Enabled;
        }
        else
        {
            //Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }

        //Handle the unlimited property array
        string[] conditionalSourceFieldArray = condHAtt.ConditionalSourceFields;
        bool[] conditionalSourceFieldInverseArray = condHAtt.ConditionalSourceFieldInverseBools;
        for (int index = 0; index < conditionalSourceFieldArray.Length; ++index)
        {
            SerializedProperty sourcePropertyValueFromArray = null;
            if (!property.isArray)
            {
                string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
                string conditionPath = propertyPath.Replace(property.name, conditionalSourceFieldArray[index]); //changes the path to the conditionalsource property path
                sourcePropertyValueFromArray = property.serializedObject.FindProperty(conditionPath);

                //if the find failed->fall back to the old system
                if (sourcePropertyValueFromArray == null)
                {
                    //original implementation (doens't work with nested serializedObjects)
                    sourcePropertyValueFromArray = property.serializedObject.FindProperty(conditionalSourceFieldArray[index]);
                }
            }
            else
            {
                // original implementation(doens't work with nested serializedObjects) 
                sourcePropertyValueFromArray = property.serializedObject.FindProperty(conditionalSourceFieldArray[index]);
            }

            //Combine the results
            if (sourcePropertyValueFromArray != null)
            {
                bool propertyEnabled = CheckPropertyType(sourcePropertyValueFromArray);
                if (conditionalSourceFieldInverseArray.Length >= (index + 1) && conditionalSourceFieldInverseArray[index]) propertyEnabled = !propertyEnabled;

                if (condHAtt.UseOrLogic)
                    enabled = enabled || propertyEnabled;
                else
                    enabled = enabled && propertyEnabled;
            }
            else
            {
                //Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
            }
        }


        //wrap it all up
        if (condHAtt.Inverse) enabled = !enabled;

        return enabled;
    }

    private bool CheckPropertyType(SerializedProperty sourcePropertyValue)
    {
        //Note: add others for custom handling if desired
        switch (sourcePropertyValue.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return sourcePropertyValue.boolValue;
            case SerializedPropertyType.ObjectReference:
                return sourcePropertyValue.objectReferenceValue != null;
            default:
                Debug.LogError("Data type of the property used for conditional renaming [" + sourcePropertyValue.propertyType + "] is currently not supported");
                return true;
        }
    }
}
