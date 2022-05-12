open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Writers
open System
open Newtonsoft.Json

type TzInfo = { tzName: string; minDiff: float; localTime: string; utcOffset: float }

let getClosest () =
    let tzs = TimeZoneInfo.GetSystemTimeZones()
    let tzList = [
        for tz in tzs do
            let localTz = TimeZoneInfo.ConvertTime(DateTime.Now, tz)
            let fivePm = DateTime(localTz.Year, localTz.Month, localTz.Day, 17, 0, 0)
            let minDifference = (localTz - fivePm).TotalMinutes
            
            yield {
                tzName = tz.StandardName;
                minDiff = minDifference
                localTime = localTz.ToString("hh:mm")
                utcOffset = tz.BaseUtcOffset.TotalHours
            }
    ] 
    
    tzList
        |> List.filter (fun (i: TzInfo) -> i.minDiff >= 0.0)
        |> List.sortBy (fun (i: TzInfo) -> i.minDiff)
        |> List.head
        
let runWebServer argv =
    let port = 8080

    let cfg =
        { defaultConfig with bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" port ] }

    let app =
        choose [ GET
                 >=> choose [ path "/" >=> request (fun _ -> OK <| JsonConvert.SerializeObject(getClosest(), Formatting.Indented)) >=> setMimeType "application/json; charset=utf-8" ] ]

    startWebServer cfg app
    0

[<EntryPoint>]
let main argv =
    runWebServer argv
    0