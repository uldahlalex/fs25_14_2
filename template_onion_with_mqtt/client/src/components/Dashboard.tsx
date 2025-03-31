import {useWsClient} from "ws-request-hook";
import {useEffect, useState} from "react";
import {Bar, BarChart, CartesianGrid, Legend, Rectangle, ResponsiveContainer, Tooltip, XAxis, YAxis} from "recharts";
import {
    AuthClient,
    Devicelog, ServerBroadcastsLiveDataToDashboard,

    StringConstants, WeatherStationClient,
} from "../generated-client.ts";
import {randomUid} from "./App.tsx";
import toast from "react-hot-toast";
const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD;

export const weatherStationClient = new WeatherStationClient(prod ? "https://" : "http://"+baseUrl);
export const authClient = new AuthClient(prod ? "https://" : "http://"+baseUrl);

export default function AdminDashboard() {

    const {onMessage, readyState, sendRequest} = useWsClient()
    const [metric, setMetrics] = useState<Devicelog[]>([])
    const [jwt, setJwt] = useState(localStorage.getItem('jwt'))
    
    useEffect(() => {
        if (jwt == null || jwt.length<1)
            return;
        weatherStationClient.getLogs(jwt).then(r => {
            setMetrics(r || []);
        })
    }, [jwt])

    useEffect(() => {
        if (readyState!=1 || jwt ==null || jwt.length<1)
            return;
        weatherStationClient.subscribeToLiveChanges(jwt, randomUid).then(r => {
            toast("welcome - you now receive live data from iot devices")
        })

        const unsub = onMessage<ServerBroadcastsLiveDataToDashboard>(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto) =>  {
            console.log(dto)
            setMetrics(dto.logs || []);
        })

        return () => {
            unsub();
        }
    }, [readyState, jwt]);


    return(<>
        {
            jwt==null &&       
            <button onClick={() => 
                authClient.register({email: Math.random()*123+"@gmail.com", password: "123456"}).then(r => {
                    toast("welcome!")
                    setJwt(r.jwt)
            })}>Click to register as a test user</button>
        }
    
        <button className="btn" onClick={() => {
            // const dto: AdminWantsToChangePreferencesForDeviceDto =
            //     {
            //         intervalMilliseconds: millis,
            //         unit: "Celcius", // yes this is hardcoded
            //         deviceId: "A" //yes, this is hardcoded
            //     }
            // httpClient.adminWantsToChangePreferencesForDevice(dto).then(resp => {
            //     toast('API sent preference change to edge devices')
            // })
        }}>Change preferences for device</button>


        <ResponsiveContainer width="100%" height={400}>
            <BarChart
                width={500}
                height={300}
                data={metric}
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