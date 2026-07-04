using UnityEngine;

namespace InteriorPlanner.Systems.Tools
{
    /// <summary>
    /// Componente de "Etiqueta" (Marker Component). 
    /// Embora esteja vazio de lógica, o seu propósito é crucial: serve como uma "Tag" segura e tipada em C#.
    /// O Balde de Tinta procura este script para saber se tem permissão para pintar a superfície.
    /// É muito mais rápido e otimizado para a CPU procurar por um Componente do que comparar strings de Tags.
    /// </summary>
    public class Paintable : MonoBehaviour
    {
        // Podes deixar vazio. Serve apenas como uma "Tag" avançada em C#
    }
}