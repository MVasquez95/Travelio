"""
Proveedor simulador 2 - formato diferente (schema B), mayor latencia
Uso: python provider_simulator_2.py --port 9002
"""
import argparse
import random
import time
from fastapi import FastAPI, HTTPException
import uvicorn

app = FastAPI()

@app.post('/search')
async def search(payload: dict):
    # Simulate higher latency and occasional partial failures
    time.sleep(random.uniform(0.2, 1.2))
    if random.random() < 0.12:
        raise HTTPException(status_code=504, detail='simulated timeout')
    offers = []
    for i in range(2):
        offers.append({
            'offer_code': f'P2-{random.randint(2000,9999)}',
            'category': 'activity',
            'cost': {'value': round(random.uniform(30, 200),2), 'curr': 'USD'},
            'seats': random.randint(0,10)
        })
    return {'provider': 'provider-2', 'results': offers}

@app.post('/reserve')
async def reserve(payload: dict):
    time.sleep(random.uniform(0.1, 0.4))
    # Simulate intermittent failure
    if random.random() < 0.15:
        return {'ok': False, 'error': 'temporarily_unavailable'}, 503
    return {'ok': True, 'reservation_ref': f'P2RES{random.randint(10000,99999)}', 'expires_in': 180}

@app.post('/confirm')
async def confirm(payload: dict):
    time.sleep(random.uniform(0.05, 0.3))
    if random.random() < 0.05:
        raise HTTPException(status_code=500, detail='internal error')
    return {'ok': True, 'booking_ref': f'P2BOOK{random.randint(10000,99999)}'}

@app.post('/revoke')
async def revoke(payload: dict):
    time.sleep(0.05)
    return {'ok': True}

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9002)
    args = parser.parse_args()
    uvicorn.run('provider_simulator_2:app', host='0.0.0.0', port=args.port, log_level='info')