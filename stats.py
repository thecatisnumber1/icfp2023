import json
import glob
import re
import statistics

rez = {}
for file in glob.glob('problems/*.json'):
    prob_num = re.findall('\d+', file)[0]
    prob = json.load(open(file))
    loc = {}
    loc['musicians'] = len(prob['musicians'])
    loc['instruments'] = len(set(prob['musicians']))
    loc['attendees'] = len(prob['attendees'])
    loc['stage'] = prob['stage_height'] * prob['stage_width']
    loc['map'] = prob['room_width'] * prob['room_height']
    tastes = []
    all_neg = len(prob['attendees'][0]['tastes'])*[True]
    for a in prob['attendees']:
        tastes += a['tastes']
        for i,taste in enumerate(a['tastes']):
            all_neg[i] &= taste < 0

    loc['average_taste'] = sum(tastes) / len(tastes)
    loc['std_dv'] = statistics.stdev(tastes)
    loc['four_loco_taste'] = len([taste for taste in tastes if taste > (loc['std_dv'] + (loc['std_dv'] * 4)) or taste < (loc['std_dv'] - (loc['std_dv'] * 4))])
    loc['max'] = max(tastes)
    loc['min'] = min(tastes)
    loc['vuvuzelas'] = len([i for i in all_neg if i])
    rez[int(prob_num)] = loc

json.dump(rez, open('stats.json', 'w'), indent=4, sort_keys=True)
