using System.Collections.Generic;
using UnityEngine;

namespace InteriorPlanner.Systems.Save
{
    [System.Serializable]
    public class FurnitureData
    {
        public string prefabID;       // Ex: "Sofa_01" ou "Divisoria_Movel"
        public Vector3 position;      // Onde está
        public Quaternion rotation;   // Para onde está virado
        public Vector3 scale;         // 📏 O tamanho (X, Y, Z) - Excelente adição!
        public string materialName;   // Cor/Textura aplicada pelo Balde de Tinta
    }

    [System.Serializable]
    public class RoomSaveData
    {
        public string projectName;
        public string lastSavedDate;
        
        public List<FurnitureData> placedObjects = new List<FurnitureData>();
    }
}   