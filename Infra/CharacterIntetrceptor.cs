using Microsoft.EntityFrameworkCore.Diagnostics;

public class CharacterInterceptor : IMaterializationInterceptor
{
    public object InitializedInstance(
        MaterializationInterceptionData data,
        object entity)
    {
        if (entity is PlayerCharacter character)
        {
			if(character.Gauges == null) {

				if(character.CurrentHealth != null && character.MaxHealth != null) {
					
					character.Gauges = new List<Gauge>();

					character.Gauges.Add(new Gauge() {
						Name="Health",
						Value=character.CurrentHealth ?? 0,
						Max=character.MaxHealth ?? 0
					});
				}
				
			}

        }
        return entity;
    }

    public object CreatedInstance(MaterializationInterceptionData data, object entity) => entity;
    public InterceptionResult InitializingInstance(MaterializationInterceptionData data, object entity, InterceptionResult result) => result;
}
