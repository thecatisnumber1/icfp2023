import json
import glob
import re

rez = {}
for file in glob.glob('problems/*.json'):
    prob_num = re.findall('\d+', file)[0]
    prob = json.load(open(file))
    acc = 0
    x_vals = [prob['stage_bottom_left'][0] + 5, prob['stage_bottom_left'][0] + prob['stage_width'] - 5]
    y_vals = [prob['stage_bottom_left'][1] + 5, prob['stage_bottom_left'][1] + prob['stage_height'] - 5]
    for i in prob['attendees']:
        close_x = sorted([i['x']] + x_vals)[1]
        close_y = sorted([i['y']] + y_vals)[1]
        dist = (((i['x'] - close_x)**2) + ((i['y'] - close_y)**2))
        for music in prob['musicians']:
            taste = i['tastes'][music]
            if taste > 0:
                t = (1_000_000 * taste) / dist
                acc += t

    rez[prob_num] = (acc)

json.dump(rez, open('ceils.json', 'w'), indent=4, sort_keys=True)
