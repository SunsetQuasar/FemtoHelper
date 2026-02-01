using MonoMod.ModInterop;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.FemtoHelper.AuspiciousInterop;

[ModImportName("auspicioushelper.templates")]
public static class TemplateIop
{
    public static class EntityParseTypes
    {
        public const int unable = 0; //will not include this entity in templates
        public const int platformbasic = 1; //basic platform; use moveV/moveH when moving
        public const int unwrapped = 2; //use this entity directly; do not put into tree
        public const int basic = 3; //basic entity; movement done via position change
        public const int removeSMbasic = 4; //basic but will remove all staticmovers on construction (use for conventionally attached items)
    }

    //Here is a template class with callbacks when stuff changes in the template. If you leave values
    //null, they will use the default implementation
    public class TemplateChildComponent : Component
    {
        public TemplateChildComponent(Entity ent) : base(false, false)
        {
            Entity = ent;
        }
        //This is a reference to the template's parent and should not be changed
        Entity parent = null;
        //Called when this is added to a template. Parent will be non-null before this function.
        //<IMPORTANT> This is called before Entity.Added is called. For maximum compatibility,
        //make sure that AddSelf returns all associated entities to this one before you return from it.
        public Action<Scene> AddTo = null;
        //This function should add your entity and any entities it makes to the provided list.
        public Action<List<Entity>> AddSelf = null;


        //Called when your entity repositions; First parameter is the new location, second parameter is the liftspeed.
        //If you have definied SetOffset, the location will be the location of the template; if you have not, I try to
        //guess a location based on your original entity's location!
        public Action<Vector2, Vector2> RepositionCB = null;
        public Action<Vector2> SetOffsetCB = null;

        //Called when the template changes visibility, collidability and active status (in order).
        //0 means no change, 1 means set to true, -1 means set to false. 
        //Note that this is the parent collidability; your component should only be actually collidable
        //if it's normal logic would have it be collidable AND the last value from these was 1.
        public Action<int, int, int> ChangeStatusCB = null;
        //You can also read from these parameters to get the current status
        public bool ParentVisible = true;
        public bool ParentCollidable = true;
        public bool ParentActive = true;

        //Called when the template this entity is a part of is destroyed. Parameter is true if particles/debris
        //should be used. Should remove the current entity and any children
        public Action<bool> DestroyCB = null;

        public void TriggerParent() => triggerTemplate(parent, Entity);
        //call this when your solids are hit please <3
        public DashCollisionResults RegisterDashhit(Player p, Vector2 dir) => registerDashhit(parent, p, dir);
        public void AddPlatform() => registerPlatform(parent, Entity);
        public Vector2 getParentLiftspeed() => getTemplateLiftspeed(parent);
    }
    public static Action<string, int, Level.EntityLoader> clarify;
    public static Action<string, Func<Level, LevelData, Vector2, EntityData, Component>> customClarify;
    public static Action<Entity, Entity> triggerTemplate;
    public static Func<Entity, Player, Vector2, DashCollisionResults> registerDashhit;
    //Call this function on any platforms your entity adds to make sure triggering propegates to the template
    public static Action<Entity, Entity> registerPlatform;
    public static Func<Entity, Vector2> getTemplateLiftspeed;
}