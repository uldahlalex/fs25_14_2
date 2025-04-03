import {useWsClient} from "ws-request-hook";
import {useEffect, useState} from "react";
import {Bar, BarChart, CartesianGrid, Legend, Rectangle, ResponsiveContainer, Tooltip, XAxis, YAxis} from "recharts";
import {
    AdminChangesPreferencesDto,
    AuthClient,
    Devicelog,
    ServerBroadcastsLiveDataToDashboard,
    StringConstants,WeatherStationClient,
} from "../generated-client.ts";
import {randomUid} from "./App.tsx";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {DeviceLogsAtom, JwtAtom} from "../atoms.ts";
import {authClient, weatherStationClient} from "../apiControllerClients.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD;



export default function AdminDashboard() {

    const {onMessage, readyState} = useWsClient()
    const [deviceLogs, setDeviceLogs] = useAtom(DeviceLogsAtom)
    const [jwt, setJwt] = useAtom(JwtAtom)
    

    //Broadcast reaction hook
    useEffect(() => {
        if (readyState!=1 || jwt ==null || jwt.length<1)
            return;
        const unsub = onMessage<ServerBroadcastsLiveDataToDashboard>(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto) =>  {
            console.log(dto)
            toast("New data from IoT device!")
            setDeviceLogs(dto.logs || []);
        })
        return () => unsub();
    }, [readyState, jwt]);


    return(<>
        {
            (jwt==null || jwt.length<1) &&       
            <button onClick={() => 
                authClient.register({email: Math.random()*123+"@gmail.com", password: "123456"}).then(r => {
                    toast("welcome!")
                    setJwt(r.jwt)
            })}>Click to register as a test user</button>
        }
    
        <button className="btn" onClick={() => {
            const dto: AdminChangesPreferencesDto =
                {
                    interval: "Minute",
                    unit: "Celcius", 
                    deviceId: "A" 
                }
            weatherStationClient.adminChangesPreferences(dto, localStorage.getItem('jwt')!).then(resp => {
                toast('API sent preference change to edge devices')
            })
        }}>Change preferences for device</button>


        <ResponsiveContainer width="100%" height={400}>
            <BarChart
                width={500}
                height={300}
                data={deviceLogs}
                margin={{
                    top: 5,
                    right: 30,
                    left: 20,
                    bottom: 5,
                }}
            >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="formattedTime" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="value" name="Temperature" fill="#8884d8" activeBar={<Rectangle fill="pink" stroke="blue" />} />
            </BarChart>
        </ResponsiveContainer>


    </>)
}