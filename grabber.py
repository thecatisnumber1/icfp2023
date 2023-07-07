import requests
import json

rez = requests.get('https://api.icfpcontest.com/problems')
num = rez.json()['number_of_problems']

for i in range(1, num + 1):
  try:
    open('problems/problem-%d.json'%(i))
  except:
    print(i)
    rez = requests.get('https://api.icfpcontest.com/problem?problem_id=%d'%(i))
    open('problems/problem-%d.json'%(i), 'w').write(rez.json()['Success'])
