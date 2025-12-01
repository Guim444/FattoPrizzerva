using UnityEngine;

public interface IAdrenalinable
{
    float Adrenaline { get; set; }
    void AddAdrenaline(float amount);
}