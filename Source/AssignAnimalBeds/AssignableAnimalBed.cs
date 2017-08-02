using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace AssignAnimalBeds
{

    public class AssignableAnimalBed : Building_Bed, IAssignableBuilding
    {
        private static FieldInfo FI_intOwnedBed = AccessTools.Field(typeof(Pawn_Ownership), "intOwnedBed");

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos()) yield return g;

            if (!this.def.building.bed_humanlike && base.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    // NOTE: this should come from somewhere else...
                    defaultLabel = "CommandBedSetOwnerLabel".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner", true),
                    defaultDesc = "CommandBedSetOwnerDesc".Translate(),
                    action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_AssignBuildingOwner(this));
                    },
                    hotKey = KeyBindingDefOf.Misc4
                };
            }
                
        }

        public new IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!base.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                return from p in Find.VisibleMap.mapPawns.AllPawns
                       where p.RaceProps.Animal && p.Faction == Faction.OfPlayer
                       select p;
            }
        }

        public new void TryAssignPawn(Pawn owner)
        {
            if (this.owners.Contains(owner))
            {
                return;
            }

            // lazy init 
            if (owner.ownership == null) owner.ownership = new Pawn_Ownership(owner);

            owner.ownership.UnclaimBed();
            if (this.owners.Count == this.MaxAssignedPawnsCount)
            {
                this.owners[this.owners.Count - 1].ownership.UnclaimBed();
            }
            this.owners.Add(owner);
            this.owners.SortBy((Pawn x) => x.thingIDNumber);
            FI_intOwnedBed.SetValue(owner.ownership, this);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();
            if (this.PlayerCanSeeOwners)
            {
                if (this.owners.Count == 0)
                {
                    stringBuilder.AppendLine("Owner".Translate() + ": " + "Nobody".Translate().ToLower());
                }
                else if (this.owners.Count == 1)
                {
                    stringBuilder.AppendLine("Owner".Translate() + ": " + this.owners[0].Label);
                }
                else
                {
                    stringBuilder.Append("Owners".Translate() + ": ");
                    bool flag = false;
                    for (int i = 0; i < this.owners.Count; i++)
                    {
                        if (flag)
                        {
                            stringBuilder.Append(", ");
                        }
                        flag = true;
                        stringBuilder.Append(this.owners[i].LabelShort);
                    }
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        // NOTE: copy pasta from Building_Bed
        private bool PlayerCanSeeOwners
        {
            get
            {
                if (base.Faction == Faction.OfPlayer)
                {
                    return true;
                }
                for (int i = 0; i < this.owners.Count; i++)
                {
                    if (this.owners[i].Faction == Faction.OfPlayer || this.owners[i].HostFaction == Faction.OfPlayer)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

    }

}
