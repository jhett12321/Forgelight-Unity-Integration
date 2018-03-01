namespace ForgelightUnity.Forgelight.Assets.Areas
{
    using System.Xml.Linq;

    public class Property
    {
        public string ID;
        public string Type;

        public XElement Parameters;
    }
}

/*Types
"RandomEffect":             id="299068848" CompositeEffectDefId="3772" MinDistance="1.000000" MaxDistance="5.000000" MinFrequency="3.000000" MaxFrequency="15.000000" OnlyPlaceInFrontOfCamera="0" PlaceOnGround="1" AreaRelative="1"
"Exclusive":                id="3342266467"
"SundererNoDeploy":         id="2982030232" Requirement="2336" FacilityId="0" DeployableClientReqId="2337"
"Thrust - No Gravity":      id="2518695024" VelocityMult="2.00"
"Interaction":              id="672212839" ProxyID="247289" Immunity="1"
"ObjectTerrainData":        id="3629103617" ObjectTerrainDataID="2005"
"Thrust - Chain":           id="1874400517" VelocityMult="2.00" JumpHeight="40.00" FacSpawnId="0" ReqSetId="0" NextChainId="170617813"
    Jump Pads, NextChainId references a landing pad also defined in area definitions.
"SoundEmitter":             id="2942660095" ActorSoundEmitterDefId="4961" effectLocOffsetX="0.000000" effectLocOffsetY="0.000000" effectLocOffsetZ="0.000000"
"CompositeEffect":          id="793311295" CompositeEffectDefId="2958" effectLocOffsetX="0.000000" effectLocOffsetY="0.000000" effectLocOffsetZ="0.000000" effectRotationH="0.000000" effectRotationP="0.000000" effectRotationR="0.000000" effectScale="1.000000"
"Death":                    id="1267736512"
    Instant Killbox. Hossin water, Amerish toggle bridges, etc.
 */
