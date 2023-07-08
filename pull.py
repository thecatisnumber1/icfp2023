import requests
import json

password = open('password').read().strip()
z = requests.post('https://api.icfpcontest.com/login', json={'username_or_email': 'The Cat is #1!!', 'password': password}).json()
headers = {'Authorization': 'Bearer ' + z['Success']}
offset = 0
limit = 100
acc = []
while True:
  rez = requests.get('https://api.icfpcontest.com/submissions', params={'offset': offset, 'limit': limit}, headers=headers).json()
  data = rez['Success']
  acc += data
  print(offset, len(data))
  if len(data) < limit:
      break
  offset += limit

bests = {}
for sub in acc:
  if 'Success' not in sub['score']:
    continue
  score = sub['score']['Success']
  id = sub['problem_id']
  if id not in bests:
    bests[id] = sub

  if score > bests[id]['score']['Success']:
    bests[id] = sub

for id in bests:
    print(id)
    sub = bests[id]
    rez = requests.get('https://api.icfpcontest.com/submission', params={'submission_id': sub['_id']}, headers=headers).json()
    sol = rez['Success']['contents']
    json.dump(sol, open('best-solves/problem-%d.json'%(id), 'w'), indent=4)
