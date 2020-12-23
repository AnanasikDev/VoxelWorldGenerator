using System;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Rotations mode; // Надо ли алгоритму поворачивать тайл (Four - необходимо добавить все варианты поворта, One - только исходный)
    public TileType type; // Необходим для сравнения тайлов
    public Side sides; // Если All, то все стороны равны, и можно указывать только ForwardSide; Если One, то надо настраивать каждую сторону поотдельности.

    public SideType[] forwardSide;
    public SideType[] backwardSide;
    public SideType[] rightSide;
    public SideType[] leftSide;

    [Range(0, 100)]
    public int Weight = 40; // Вес, с которым будет спавниться этот тайл. (Частота появления)

    // Todo: сделать тэги на каждую сторону
    public bool IsEquals(Tile other)
    {
        return type == other.type && transform.rotation == other.transform.rotation;
    }
    private void Start()
    {
        /*if (sides == Side.All)
        {
            backwardSide = rightSide = leftSide = forwardSide;
        }*/
    }
    public void Rotate(int angle=90)
    {
        switch (angle)
        {
            case 0:
                break;
            case 90:
                Rotate90();
                break;
            case 180:
                Rotate90();
                Rotate90();
                break;
            case 270:
                Rotate90();
                Rotate90();
                Rotate90(); 
                break;
        }
    }
    private void Rotate90()
    {
        SideType[] DeepCopy(SideType[] array)
        {
            SideType[] copy = new SideType[array.Length];
            int i = 0;
            foreach (SideType obj in array)
            {
                copy[i] = obj;
                i++;
            }
            return copy;
        }
        transform.Rotate(0, 90, 0);
        var f = DeepCopy(forwardSide); 
        var b = DeepCopy(backwardSide); 
        var r = DeepCopy(rightSide); 
        var l = DeepCopy(leftSide);
        /*forwardSide = r; 
        rightSide = b; 
        backwardSide = l; 
        leftSide = f;*/
        forwardSide = l;
        rightSide = f;
        backwardSide = r;
        leftSide = b;
        //(forwardSide, rightSide, backwardSide, leftSide) = (rightSide, backwardSide, leftSide, forwardSide);
    }

    public enum SideType
    {
        Grass,
        Water
        //GrassWater,
        //WaterGrass // Относительно взгляда из центра объекта.
    }
    public enum Side
    {
        All, // Все стороны равны
        One // Все стороны разные
    }
    public enum Rotations
    {
        Four, // 4 поворота
        Two, // 2 поворота
        One // 1 поворот
    }
    public enum TileType
    {
        Water,
        Grass,
        GrassWithTree,
        Corner,
        InnerCorner,
        Side
    }
}