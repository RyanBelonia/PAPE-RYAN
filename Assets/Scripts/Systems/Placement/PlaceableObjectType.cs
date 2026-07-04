namespace InteriorPlanner.Systems.Placement
{
    /// <summary>
    /// Categoriza a natureza física e estrutural do objeto no mundo 3D.
    /// Define como o motor de física e o gravador de saves devem tratar esta entidade.
    /// </summary>
    public enum PlaceableObjectType
    {
        Furniture, // Móvel livre de chão
        Divider,   // Parede interna / Divisória
        Door,      // Exige uma parede hospedeira, corta a navegação
        Window     // Exige uma parede hospedeira, flutua
    }
}