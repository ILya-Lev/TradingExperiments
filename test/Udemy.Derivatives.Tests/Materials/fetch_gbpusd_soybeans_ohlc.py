import csv
import datetime
import io
import json
import os
import urllib.parse
import urllib.request


def fetch_yahoo_ohlc(ticker: str) -> list[tuple[str, float, float, float, float]]:
    url = (
        f"https://query1.finance.yahoo.com/v8/finance/chart/{urllib.parse.quote(ticker)}"
        "?period1=-1325635200&period2=2000000000&interval=1d"
    )
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=20) as resp:
        data = json.loads(resp.read().decode("utf-8"))

    res = data["chart"]["result"][0]
    ts = res["timestamp"]
    q = res["indicators"]["quote"][0]
    epoch = datetime.datetime(1970, 1, 1, tzinfo=datetime.timezone.utc)

    rows: list[tuple[str, float, float, float, float]] = []
    seen_dates: set[str] = set()
    for i in range(len(ts)):
        dt = (epoch + datetime.timedelta(seconds=ts[i])).strftime("%Y-%m-%d")
        o, h, l, c = q["open"][i], q["high"][i], q["low"][i], q["close"][i]
        if o is None or h is None or l is None or c is None:
            continue
        if dt not in seen_dates:
            seen_dates.add(dt)
            rows.append((dt, float(o), float(h), float(l), float(c)))
    rows.sort(key=lambda x: x[0])
    return rows


def fetch_pre2003_gbpusd_spot() -> list[tuple[str, float, float, float, float]]:
    url = "https://www.eco3min.fr/dataset/gbp-usd-exchange-rate.csv"
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=20) as resp:
        text = resp.read().decode("utf-8", errors="ignore")

    rows: list[tuple[str, float, float, float, float]] = []
    reader = csv.DictReader(io.StringIO(text))
    for r in reader:
        dt = (r.get("date") or "").strip()
        val_str = (r.get("gbp_usd") or "").strip()
        if not dt or not val_str:
            continue
        try:
            v = float(val_str)
        except ValueError:
            continue
        rows.append((dt, v, v, v, v))
    rows.sort(key=lambda x: x[0])
    return rows


def fetch_pre2000_soybeans() -> list[tuple[str, float, float, float, float]]:
    url = "https://raw.githubusercontent.com/robcarver17/pysystemtrade/master/data/futures/multiple_prices_csv/SOYBEAN.csv"
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=20) as resp:
        text = resp.read().decode("utf-8", errors="ignore")

    by_date: dict[str, float] = {}
    reader = csv.DictReader(io.StringIO(text))
    for r in reader:
        dt_raw = (r.get("DATETIME") or "").strip()
        p_raw = (r.get("PRICE") or "").strip()
        if not dt_raw or not p_raw:
            continue
        dt = dt_raw[:10]
        try:
            p = float(p_raw)
        except ValueError:
            continue
        by_date[dt] = p

    rows = [(dt, p, p, p, p) for dt, p in sorted(by_date.items())]
    return rows


def write_csv(
    path: str,
    rows: list[tuple[str, float, float, float, float]],
    decimals: int,
) -> int:
    fmt = f"{{:.{decimals}f}}"
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("day,O,H,L,C\n")
        for dt, o, h, l, c in rows:
            f.write(
                f"{dt},{fmt.format(o)},{fmt.format(h)},{fmt.format(l)},{fmt.format(c)}\n"
            )
    return len(rows)


def build_datasets(output_dir: str) -> None:
    os.makedirs(output_dir, exist_ok=True)
    cutoff_30y = "1996-11-07"

    # 1. GBP/USD
    yf_gbp = fetch_yahoo_ohlc("GBPUSD=X")
    first_yf_gbp_date = yf_gbp[0][0]
    pre_gbp = [r for r in fetch_pre2003_gbpusd_spot() if r[0] < first_yf_gbp_date]
    full_gbp = pre_gbp + yf_gbp
    gbp_30y = [r for r in full_gbp if r[0] >= cutoff_30y]

    n_gbp_pure = write_csv(
        os.path.join(output_dir, "gbpusd_daily_ohlc_pure_2003_2026.csv"), yf_gbp, 4
    )
    n_gbp_30y = write_csv(
        os.path.join(output_dir, "gbpusd_daily_ohlc_30y_1996_2026.csv"), gbp_30y, 4
    )
    n_gbp_full = write_csv(
        os.path.join(output_dir, "gbpusd_daily_ohlc_full_1971_2026.csv"), full_gbp, 4
    )

    # 2. Soybeans
    yf_soy = fetch_yahoo_ohlc("ZS=F")
    first_yf_soy_date = yf_soy[0][0]
    pre_soy = [r for r in fetch_pre2000_soybeans() if r[0] < first_yf_soy_date]
    full_soy = pre_soy + yf_soy
    soy_30y = [r for r in full_soy if r[0] >= cutoff_30y]

    n_soy_pure = write_csv(
        os.path.join(output_dir, "soybeans_daily_ohlc_pure_2000_2026.csv"), yf_soy, 2
    )
    n_soy_30y = write_csv(
        os.path.join(output_dir, "soybeans_daily_ohlc_30y_1996_2026.csv"), soy_30y, 2
    )
    n_soy_full = write_csv(
        os.path.join(output_dir, "soybeans_daily_ohlc_full_1985_2026.csv"), full_soy, 2
    )

    print(f"GBPUSD pure: {n_gbp_pure} ({yf_gbp[0][0]} -> {yf_gbp[-1][0]})")
    print(f"GBPUSD 30y:  {n_gbp_30y} ({gbp_30y[0][0]} -> {gbp_30y[-1][0]})")
    print(f"GBPUSD full: {n_gbp_full} ({full_gbp[0][0]} -> {full_gbp[-1][0]})")
    print(f"Soybeans pure: {n_soy_pure} ({yf_soy[0][0]} -> {yf_soy[-1][0]})")
    print(f"Soybeans 30y:  {n_soy_30y} ({soy_30y[0][0]} -> {soy_30y[-1][0]})")
    print(f"Soybeans full: {n_soy_full} ({full_soy[0][0]} -> {full_soy[-1][0]})")


if __name__ == "__main__":
    build_datasets(os.path.dirname(os.path.abspath(__file__)))
