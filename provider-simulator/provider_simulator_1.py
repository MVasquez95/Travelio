"""
Proveedor simulador 1 - Fast, returns JSON in schema A
Uso: python provider_simulator_1.py --port 9001
"""
import argparse
import random
import time
from fastapi import FastAPI, HTTPException, Request
from pydantic import BaseModel
import uvicorn

app = FastAPI()
bookings_by_key = {}

class SearchReq(BaseModel):
    origin: str
    destination: str
    start_date: str
    end_date: str
    passengers: int = 1

@app.post('/search')
async def search(req: SearchReq):
    # Simulate latency and occasional errors
    delay = random.uniform(0.05, 0.5)
    time.sleep(delay)
    if random.random() < 0.05:
        raise HTTPException(status_code=500, detail='simulated provider error')
    offers = []
    for i in range(3):
        offers.append({
            'id': f'p1-offer-{random.randint(1000,9999)}',
            'type': 'hotel',
            'price': round(random.uniform(80, 400),2),
            'currency': 'USD',
            'availability': random.randint(0,5),
            'details': {'rate_plan': 'standard'}
        })
    return {'provider': 'provider-1', 'offers': offers}

@app.post('/prebook')
async def prebook(payload: dict):
    # Simulate hold success with a prebooking id
    time.sleep(random.uniform(0.05, 0.2))
    if random.random() < 0.1:
        # simulate no availability
        return {'success': False, 'reason': 'no_availability'}, 409
    pre_id = f'p1-pre-{random.randint(10000,99999)}'
    return {'success': True, 'prebooking_id': pre_id, 'hold_seconds': 300}

@app.post('/book')
async def book(payload: dict):
    time.sleep(random.uniform(0.05, 0.3))
    if random.random() < 0.08:
        raise HTTPException(status_code=502, detail='simulated gateway error')
    key = payload.get('idempotencyKey')
    if key in bookings_by_key:
        return {'success': True, 'booking_id': bookings_by_key[key]}
    booking_id = f'p1-book-{random.randint(10000,99999)}'
    if key:
        bookings_by_key[key] = booking_id
    return {'success': True, 'booking_id': booking_id}

@app.post('/cancel')
async def cancel(payload: dict):
    time.sleep(0.05)
    return {'success': True}

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9001)
    args = parser.parse_args()
    uvicorn.run('provider_simulator_1:app', host='0.0.0.0', port=args.port, log_level='info')
