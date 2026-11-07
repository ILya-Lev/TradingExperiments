import datetime
import json
import os
import urllib.request

def fetch_sp500_ohlc(output_dir: str) -> None:
    url = (
        "https://query1.finance.yahoo.com/v8/finance/chart/%5EGSPC"
        "?period1=-1325635200&period2=2000000000&interval=1d"
    )
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=15) as resp:
        data = json.loads(resp.read().decode("utf-8"))

    res = data["chart"]["result"][0]
    ts = res["timestamp"]
    q = res["indicators"]["quote"][0]
    epoch = datetime.datetime(1970, 1, 1, tzinfo=datetime.timezone.utc)

    os.makedirs(output_dir, exist_ok=True)
    full_path = os.path.join(output_dir, "sp500_daily_ohlc_full_1927_2026.csv")
    last30y_path = os.path.join(output_dir, "sp500_daily_ohlc_30y_1996_2026.csv")
    cutoff_30y = "1996-11-07"

    with open(full_path, "w", encoding="utf-8", newline="\n") as f_full, \
         open(last30y_path, "w", encoding="utf-8", newline="\n") as f_30y:
        f_full.write("day,O,H,L,C\n")
        f_30y.write("day,O,H,L,C\n")
        for i in range(len(ts)):
            dt = (epoch + datetime.timedelta(seconds=ts[i])).strftime("%Y-%m-%d")
            o, h, l, c = q["open"][i], q["high"][i], q["low"][i], q["close"][i]
            if o is None or h is None or l is None or c is None:
                continue
            line = f"{dt},{o:.2f},{h:.2f},{l:.2f},{c:.2f}\n"
            f_full.write(line)
            if dt >= cutoff_30y:
                f_30y.write(line)

if __name__ == "__main__":
    fetch_sp500_ohlc(os.path.dirname(os.path.abspath(__file__)))
