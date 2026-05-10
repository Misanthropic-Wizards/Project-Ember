suspicion-role-greeting = Вы { $roleName }!

suspicion-objective = Цель: { $objectiveText }

suspicion-partners-in-crime =
    { $partnersCount ->
        [zero] You're on your own. Good luck!
        [one] Your partner in crime is { $partnerNames }.
       *[other] Your partners in crime are { $partnerNames }.
    }
