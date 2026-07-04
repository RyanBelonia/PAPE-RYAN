using System.Collections.Generic;
using UnityEngine;

namespace InteriorPlanner.Systems.Save
{
    /// <summary>
    /// O esqueleto de dados de um objeto individual para ser guardado. 
    /// O atributo [System.Serializable] transforma as variáveis abaixo num texto JSON.
    /// </summary>
    [System.Serializable]
    public class FurnitureData
    {
        public string prefabID;       // Ex: "Sofa_01" ou "Divisoria_Movel". A chave para a reconstrução.
        public Vector3 position;      // Onde está
        public Quaternion rotation;   // Para onde está virado matematicamente
        public Vector3 scale;         // O tamanho (X, Y, Z) - Excelente adição para Divisórias esticadas!
        public string materialName;   // Cor/Textura aplicada pelo Balde de Tinta. ("Default" se não foi pintado)
    }

    /// <summary>
    /// O envelope "Raiz" do ficheiro de Save. É esta classe que engloba tudo o resto 
    /// e é gravada diretamente no disco rígido com extensão .json
    /// </summary>
    [System.Serializable]
    public class RoomSaveData
    {
        public string projectName;
        public string lastSavedDate; // Registo de tempo (Timestamp) para organizar saves no futuro
        
        // A lista principal que vai guardar as centenas de móveis, portas e janelas que o utilizador criou
        public List<FurnitureData> placedObjects = new List<FurnitureData>();
    }
}