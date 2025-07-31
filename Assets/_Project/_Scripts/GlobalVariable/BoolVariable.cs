using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Global Variable/Bool", fileName = "New Bool Variable")]
public class BoolVariable : ScriptableObject
{
    private bool _value;
    public event Action<bool> OnValueChanged;
    
    public bool Value
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