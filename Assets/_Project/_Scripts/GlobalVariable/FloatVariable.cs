using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Global Variable/Float", fileName = "New Float Variable")]
public class FloatVariable : ScriptableObject
{
    private float _value;
    public event Action<float> OnValueChanged;
    
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }
}