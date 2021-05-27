using Sagede.OfficeLine.Shared.Customizing;
using System;
using System.Linq;

namespace incluvision.Charge.Dcm
{
    public class DcmListener : IDcmCallback
    {
        /// <summary>
        /// DCM-Listner
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Entry(IDcmContext context)
        {
            var startDcmCall = Sagede.Core.Diagnostics.HighResolutionTimer.Now();

            try
            {
                TraceLog.LogVerbose("Dcm-Ausführung gestartet => {0}", context.Listname);

                switch (context.ListId)
                {

                    #region VKBeleg


                    case DcmDefinitionManager.DcmListId.VKBelegProxyAfterTransform:
                        {

                            var transformCtx = (Sagede.OfficeLine.Wawi.Services.DcmContextBelegProxyAfterTransform)context;
                            // Alle chargenpflichtigen Artikelpositionen laden
                            foreach (var pos in transformCtx.Beleg.Positionen.Where(x =>
                                    x.Positionstyp == Sagede.OfficeLine.Wawi.Tools.Positionstyp.Artikel
                                    && x.ChargenPflicht >= 2))
                            {
                                // Lagerplätze zum Artikel laden
                                if (pos.LagerplatzLoad.Any())
                                {
                                    // Es müssten an dieser Stelle immer Lagerplätze vorhanden sein,
                                    // andernfalls besteht kein Bestand und es kann auch keine Charge zuordnet werden
                                    if (pos.Lagerplatz.Any())
                                    {
                                        // Hier deinen Code einfügen, um die Charge zu ermitteln, im Beispiel nehme ich den 
                                        // erstbesten Lagerplatz
                                        var lp = pos.Lagerplatz.First();
                                        //Und füge für den von mir im Beispiel verwendeten Artikel 10200030 / Strukturtapete 
                                        //aus der Max Mustermann Demo-Datenbank und füge die Charge "Samtweiß matt" hinzu
                                        lp.ChargenCollection.Add(new Sagede.OfficeLine.Wawi.LagerEngine.ChargenEintrag()
                                        {
                                            Charge = "Samtweiß matt",
                                            Menge = pos.Menge,
                                            //Kein FiFo-Artikel
                                            BestandsHandle = 0
                                        });
                                        // Wichtig! 
                                        lp.BestandBuchen = true;
                                        pos.SetIstDirtyLagerbuchung(true);
                                    }



                                }

                            }
                            break;
                        }


                    #endregion VKBeleg


                    default:
                        break;
                }

                TraceLog.LogVerbose("Dcm-Ausführung beendet => {0}", context.Listname);
                TraceLog.LogTime("Dcm-Ausführung", startDcmCall);
                return true;
            }
            catch (Exception ex)
            {
                TraceLog.LogException("Fehler in Dcm-Ausführung.");
                TraceLog.LogException(ex);
                return false;
            }
        }
    }
}