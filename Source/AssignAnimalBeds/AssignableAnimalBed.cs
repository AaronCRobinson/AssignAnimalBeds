using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Harmony;
using RimWorld;

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
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandBedSetAsMedicalLabel".Translate(),
                    defaultDesc = "CommandBedSetAsMedicalDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AsMedical", true),
                    isActive = (() => this.Medical),
                    toggleAction = delegate
                    {
                        this.Medical = !this.Medical;
                    },
                    hotKey = KeyBindingDefOf.Misc2
                };

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

            /*if (this.PlayerCanSeeOwners)
            {
                stringBuilder.AppendLine("ForColonistUse".Translate());
            }*/

            if (this.Medical)
            {
                stringBuilder.AppendLine("MedicalBed".Translate());
                stringBuilder.AppendLine("RoomSurgerySuccessChanceFactor".Translate() + ": " + this.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.SurgerySuccessChanceFactor).ToStringPercent());
                stringBuilder.AppendLine("RoomInfectionChanceFactor".Translate() + ": " + this.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.InfectionChanceFactor).ToStringPercent());
            }
            else if (this.PlayerCanSeeOwners)
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

        /*public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!myPawn.RaceProps.Humanlike && this.Medical)
            {
                if (!HealthAIUtility.ShouldSeekMedicalRest(myPawn) && !HealthAIUtility.ShouldSeekMedicalRestUrgent(myPawn))
                {
                    yield return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NotInjured".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else
                {
                    Action sleep = delegate
                    {
                        if (this.Medical && myPawn.CanReserveAndReach(this, PathEndMode.ClosestTouch, Danger.Deadly, this.SleepingSlotsCount, -1, null, true))
                        {
                            Job job = new Job(JobDefOf.LayDown, this) { restUntilHealed = true };
                            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            myPawn.mindState.ResetLastDisturbanceTick();
                        }
                    };
                    if (this.AnyUnoccupiedSleepingSlot)
                    {
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("UseMedicalBed".Translate(), sleep, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                    }
                    else
                    {
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("UseMedicalBed".Translate(), sleep, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "SomeoneElseSleeping");
                    }
                }
            }
        }*/

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && this.PlayerCanSeeOwners)
            {
                Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
                /*if (!this.owners.Any<Pawn>())
                {
                    GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
                }
                else */ // NOTE: only show owned.
                if (this.owners.Count == 1)
                {
                    if (this.owners[0].InBed() && this.owners[0].CurrentBed() == this) 
                    {
                        return;
                    }
                    GenMapUI.DrawThingLabel(this, this.owners[0].NameStringShort, defaultThingLabelColor);
                }
                // NOTE: use this code later for multi.
                /*else
                {
                    for (int i = 0; i < this.owners.Count; i++)
                    {
                        if (!this.owners[i].InBed() || this.owners[i].CurrentBed() != this || !(this.owners[i].Position == this.GetSleepingSlotPos(i)))
                        {
                            Vector3 multiOwnersLabelScreenPosFor = this.GetMultiOwnersLabelScreenPosFor(i);
                            GenMapUI.DrawThingLabel(multiOwnersLabelScreenPosFor, this.owners[i].NameStringShort, defaultThingLabelColor);
                        }
                    }
                }*/
            }
        }

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

        private FieldInfo FI_medicalInt = AccessTools.Field(typeof(Building_Bed), "medicalInt");
        public new bool Medical
        {
            get
            {
                return (bool)FI_medicalInt.GetValue(this);
            }
            set
            {
                if (value == this.Medical || this.def.building.bed_humanlike)
                {
                    return;
                }
                Traverse t = Traverse.Create(this);
                t.Method("RemoveAllOwners").GetValue(); //this.RemoveAllOwners();
                FI_medicalInt.SetValue(this , value);
                this.Notify_ColorChanged();
                if (base.Spawned)
                {
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                    t.Method("NotifyRoomBedTypeChanged").GetValue(); //this.NotifyRoomBedTypeChanged();
                }
                t.Method("FacilityChanged").GetValue(); //this.FacilityChanged();
            }
        }

    }

}
