import {useWsClient} from "ws-request-hook";
import {useEffect} from "react";
import {Bar, BarChart, CartesianGrid, Legend, Rectangle, ResponsiveContainer, Tooltip, XAxis, YAxis} from "recharts";
import {ServerBroadcastsLiveDataToDashboard, StringConstants,} from "../generated-client.ts";
import toast from "react-hot-toast";
import {useAtom} from "jotai";
import {DeviceLogsAtom, JwtAtom} from "../atoms.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD;


export default function AdminDashboard() {

    const {onMessage, readyState} = useWsClient()
    const [deviceLogs, setDeviceLogs] = useAtom(DeviceLogsAtom)
    const [jwt, setJwt] = useAtom(JwtAtom)

    if (!jwt || jwt.length < 1) {
        return (<div className="flex flex-col items-center justify-center h-screen">please sign in to continue</div>)
    }

    //Broadcast reaction hook
    useEffect(() => {
        if (readyState != 1 || jwt == null || jwt.length < 1)
            return;
        const unsub = onMessage<ServerBroadcastsLiveDataToDashboard>(StringConstants.ServerBroadcastsLiveDataToDashboard, (dto) => {
            console.log(dto)
            toast("New data from IoT device!")
            setDeviceLogs(dto.logs || []);
        })
        return () => unsub();
    }, [readyState, jwt]);


    return (<>


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
                <CartesianGrid strokeDasharray="3 3"/>
                <XAxis dataKey="formattedTime"/>
                <YAxis/>
                <Tooltip/>
                <Legend/>
                <Bar dataKey="value" name="Temperature" fill="#8884d8"
                     activeBar={<Rectangle fill="pink" stroke="blue"/>}/>
            </BarChart>
        </ResponsiveContainer>


    </>)
}