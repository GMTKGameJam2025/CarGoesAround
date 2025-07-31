using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Global Variable/Int", fileName = "New Int Variable")]
public class IntVariable : ScriptableObject
{
    private int _value;
    
    public int Value
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

    public event Action<int> OnValueChanged;
}